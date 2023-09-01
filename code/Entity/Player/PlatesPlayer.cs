using Sandbox;
using System.ComponentModel;

namespace PlatesGame;

public partial class PlatesPlayer : AnimatedEntity
{
	
	/// <summary>
	/// Plate variables 
	/// </summary>
	[Net] public PlateEntity OwnedPlate { get; set; }
	[Net] public bool WasImpacted { get; set; } 

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
	
	public override void ClientSpawn()
	{
		base.ClientSpawn();
		_ = new UI.PlayerTag( this );
	}
	
	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );
		
		EnableTouch = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public void Respawn()
	{
		Components.Create<PawnController>();
		Components.Create<PawnAnimator>();
		Components.GetOrCreate<PawnCamera>();
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Health = 100f;
		
		Tags.Add("player");
		LifeState = LifeState.Alive;
		Controller.Gravity = GameConfig.DefaultGravity;
	}

	[GameEvent.Tick]
	private void Tick()
	{
		if ( Game.IsServer )
		{
			if ( Position.z <= -2000f && LifeState is LifeState.Alive)
			{
				if ( PlatesGame.CurrentState is WaitingState )
				{
					PlateManager.ReturnPlayerToPlate( this );
					return;
				}
				Log.Info($"{Client.Name} has died"  );
				Kill();
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
		var killPlate = PlatesGame.CurrentState?.OnPlayerDeath(this);
		if ( killPlate is not null && killPlate == false )
			return;
		
		OwnedPlate?.Kill();
		Components.GetOrCreate<SpectatorCamera>();
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
}
