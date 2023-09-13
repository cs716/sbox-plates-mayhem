using System;
using System.Collections.Generic;
using Sandbox;
using System.ComponentModel;
using System.Linq;

namespace PlatesGame;

public partial class PlatesPlayer : AnimatedEntity
{
	
	/// <summary>
	/// Plate variables 
	/// </summary>
	[Net] public PlateEntity OwnedPlate { get; set; }

	private bool IsThirdPerson { get; set; } = true;
	
	[ClientInput]
	public Vector3 InputDirection { get; set; }
	
	[ClientInput]
	public Angles ViewAngles { get; set; }

	/// <summary>
	/// Position a player should be looking from in world space.
	/// </summary>
	[Browsable( false )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
	/// </summary>
	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	public BBox Hull
	{
		get => new
		(
			new Vector3( -16, -16, 0 ),
			new Vector3( 16, 16, 64 )
		);
	}

	[BindComponent] public PawnController Controller { get; }
	[BindComponent] public PawnAnimator Animator { get; }
	[BindComponent] public BasePlayerCamera Camera { get; }

	public override Ray AimRay => new ( EyePosition, EyeRotation.Forward );

	public enum PlayerModifier
	{
		Poisoned
	}

	[Net] public IList<PlayerModifier> PlayerModifiers { get; set; }

	public void AddModifier( PlayerModifier modifier )
	{
		if ( !PlayerModifiers.Contains( modifier ) )
			PlayerModifiers.Add( modifier );
		
		ModifiersUpdated();
	}
	
	public void RemoveModifier( PlayerModifier modifier )
	{
		if ( PlayerModifiers.Contains( modifier ) )
			PlayerModifiers.Remove( modifier );

		ModifiersUpdated();
	}

	private void ModifiersUpdated()
	{
		PoisonTick = Random.Shared.Float( 10f, 30f ); 
	}
	
	public override void ClientSpawn()
	{
		base.ClientSpawn();
		_ = new UI.PlayerTag( this );
	}
	
	public override void Spawn()
	{
		Transmit = TransmitType.Always;
		SetModel( "models/citizen/citizen.vmdl" );
		
		Tags.Add( "player", "trigger" );
		
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		
		EnableHitboxes = true;
		EnableDrawing = true; 
		EnableTouch = true;

		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Hull.Mins, Hull.Maxs );

		if ( Game.IsServer )
		{
			PlayerModifiers = new List<PlayerModifier>();
		}
	}

	public void Respawn()
	{
		Components.Create<PawnController>();
		Components.Create<PawnAnimator>();
		Components.Create<PawnCamera>();
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		PlayerModifiers.Clear();
		Health = 100f;
		LifeState = LifeState.Alive;
	}

	private RealTimeUntil PoisonTick;
	[GameEvent.Tick]
	private void Tick()
	{
		if ( Game.IsServer )
		{
			if ( Position.z is <= -2000f or >= 5000f && LifeState is LifeState.Alive)
			{
				if ( PlatesGame.CurrentState is WaitingState )
				{
					PlateManager.ReturnPlayerToPlate( this );
					return;
				}
				Log.Info($"{Client.Name} has died"  );
				Kill();
			}

			if ( PlayerModifiers.Contains( PlayerModifier.Poisoned ) && PoisonTick )
			{
				PoisonTick = Random.Shared.Float( 5f, 30f ); 
				// A-choo 
				Sound.FromEntity( "player.sneeze", this );
				
				// Spread the rona 
				foreach (var player in FindInSphere( Position, 100f  ).OfType<PlatesPlayer>().Where( p => p.LifeState is LifeState.Alive ))
				{
					if (!player.PlayerModifiers.Contains(PlayerModifier.Poisoned  ))
						player.AddModifier( PlayerModifier.Poisoned );
				}
				
				// Damage 
				TakeDamage (DamageInfo.Generic( 5f ));
			}
		}
		else
		{
			EnableDrawing = LifeState is LifeState.Alive;
		}
	}

	public void Kill()
	{
		if ( LifeState is LifeState.Alive ) // Player is alive in the game - Send to spectate
		{
			OnKilled();
		}
	}

	public override void OnKilled()
	{
		LifeState = LifeState.Dead;
		PlatesGame.CurrentState?.OnPlayerDeath(this);
		
		OwnedPlate?.Kill();
		Components.Create<SpectatorCamera>();
	}

	public void DressFromClient( IClient cl )
	{
		var c = new ClothingContainer();
		c.LoadFromClient( cl );
		c.DressEntity( this );
	}
	
	public override void Simulate( IClient cl )
	{
		SimulateRotation();
		Controller?.Simulate( cl );
		Animator?.Simulate();
		//EyeLocalPosition = Vector3.Up * (64f * Scale);
	}
	
	public override void FrameSimulate( IClient cl )
	{
		SimulateRotation();
		Camera?.Update();
	}
	
	public override void BuildInput()
	{
		Camera?.BuildInput();
	}
	protected void SimulateRotation()
	{
		EyeRotation = ViewAngles.ToRotation();
		Rotation = ViewAngles.WithPitch( 0f ).ToRotation();
	}
	
	public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Hull.Mins, Hull.Maxs, liftFeet );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start, end )
			.Size( mins, maxs )
			.WithAnyTags( "solid", "playerclip", "passbullets" )
			.Ignore( this )
			.Run();

		return tr;
	}
	
	TimeSince timeSinceLastFootstep = 0;
	public override void OnAnimEventFootstep( Vector3 position, int foot, float volume )
	{
		if ( !Game.IsServer )
			return;
		
		if ( LifeState != LifeState.Alive )
			return;

		if ( timeSinceLastFootstep < 0.18f )
			return;
		
		volume *= Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 0.1f;
		timeSinceLastFootstep = 0;
		
		var tr = Trace.Ray( position, position + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume * 20 );
	}
}
