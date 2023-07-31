using Sandbox;
using System.ComponentModel;
using Pl8Mayhem.entity;
using Sandbox.Component;

namespace Pl8Mayhem;

public partial class PlatesPlayer : AnimatedEntity
{
	[Net] private Glow Glow {get; set;}

	[Net] public bool Alive { get; set; } = false;

	[Net] public PlateEntity OwnedPlate { get; set; }
	
	private bool IsThirdPerson { get; set; } = true;
	
	[Net, Predicted]
	//public Weapon ActiveWeapon { get; set; }

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
	//[BindComponent] public PawnAnimator Animator { get; }

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	public override void Spawn()
	{
		Components.Create<PawnController>();
		//Components.Create<PawnAnimator>();
		SetModel( "models/citizen/citizen.vmdl" );
		EnableDrawing = false;
	}

	public void Respawn()
	{
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		
		Components.Create<Glow>();
		DressFromClient( Client );
	}

	[GameEvent.Tick.Server]
	private void Tick()
	{
		if ( Position.z <= -100f && Alive)
		{
			Alive = false;
			Kill();
		}
	}

	public void Kill()
	{
		if ( Alive ) // Player is alive in the game - Send to spectate
		{
			OnKilled();
		}
		else // Player died while spectating. Noob down
		{
			
		}
	}

	public override void OnKilled()
	{
		base.OnKilled();
		
		Alive = false;
		OwnedPlate?.Kill();
		//Pl8Mayhem.State?.OnPlayerDeath(this);
	}

	private void DressFromClient( IClient cl )
	{
		var c = new ClothingContainer();
		c.LoadFromClient( cl );
		c.DressEntity( this );
	}

	public void ResetValues(bool changeProperties = true)
	{
		SetGlow(false);
		if ( !changeProperties )
		{
			return;
		}

		Scale = 1.0f;
		RenderColor = Color.White;
		Velocity = Vector3.Zero;
	}
	
	public override void Simulate( IClient cl )
	{
		SimulateRotation();
		Controller?.Simulate(  );
		//Animator?.Simulate();
		//ActiveWeapon?.Simulate( cl );
		EyeLocalPosition = Vector3.Up * (64f * Scale);
	}
	
	public override void FrameSimulate( IClient cl )
	{
		SimulateRotation();

		Camera.Rotation = ViewAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		if ( Input.Pressed( "view" ) )
		{
			IsThirdPerson = !IsThirdPerson;
		}

		if ( IsThirdPerson )
		{
			var pos = Position + Vector3.Up * 64;
			var rot = Camera.Rotation * Rotation.FromAxis( Vector3.Up, -16 );

			var distance = 80.0f * Scale;
			var targetPos = pos + rot.Right * ((CollisionBounds.Mins.x + 50) * Scale);
			targetPos += rot.Forward * -distance;

			var tr = Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.Ignore( this )
				.Radius( 8 )
				.Run();
			
			Camera.FirstPersonViewer = null;
			Camera.Position = tr.EndPosition;
		}
		else
		{
			Camera.FirstPersonViewer = this;
			Camera.Position = EyePosition;
		}
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
	
	public override void BuildInput()
	{
		
		DebugOverlay.ScreenText($"Input: {Input.AnalogMove} ", 1);
		InputDirection = Input.AnalogMove;

		if ( Input.StopProcessing )
			return;
		
		DebugOverlay.ScreenText($"Input: {Input.AnalogMove} ", 2);

		var look = Input.AnalogLook;

		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
		{
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;
		
		Controller?.BuildInput();
	}

	public void SetGlow(bool visible, Color color = default)
	{
		if(color != default)
		{
			Glow.Color = color;
		}
		Glow.Enabled = visible;
	}

	private void SimulateRotation()
	{
		EyeRotation = ViewAngles.ToRotation();
		Rotation = ViewAngles.WithPitch( 0f ).ToRotation();
	}
}
