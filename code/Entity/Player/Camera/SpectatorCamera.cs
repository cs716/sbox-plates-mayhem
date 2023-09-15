using System.Linq;
using Sandbox;

namespace PlatesGame;

public class SpectatorCamera : BasePlayerCamera
{
	private enum SpectatorCameraMode
	{
		Freecam,
		Spectate
	}

	private SpectatorCameraMode CameraMode = SpectatorCameraMode.Freecam;
	
	/// <summary>
	/// Variables for FreeCamera
	/// </summary>
	private const int BaseMoveSpeed = 300;
	private float _moveSpeed = 1f;
	private Angles _lookAngles = Camera.Rotation.Angles();
	private Vector3 _moveInput;
	
	/// <summary>
	/// Variables for Spectator camera
	/// </summary>
	private static float WheelSpeed => 30f;
	private static Vector2 CameraDistance => new(125, 1000);
	private static Vector2 PitchClamp => new(-80, 80);

	private float OrbitDistance = 400f;
	private float TargetOrbitDistance = 400f;
	private Angles OrbitAngles = Angles.Zero;
	private PlatesPlayer _currentFollowingPawn;
	
	public override void BuildInput()
	{
		_moveSpeed = 1f;
		_moveInput = Input.AnalogMove;

		if ( CameraMode == SpectatorCameraMode.Freecam )
		{
			if ( Input.Down( "run" ) )
				_moveSpeed = 5f;

			if ( Input.Down( "duck" ) )
				_moveSpeed = 0.2f;

			if ( Input.Down( "jump" ) )
				_moveInput += Vector3.Up * 30f;
		}
		else
		{
			var wheel = Input.MouseWheel;
			if ( wheel != 0 )
			{
				TargetOrbitDistance -= wheel * WheelSpeed;
				TargetOrbitDistance = TargetOrbitDistance.Clamp( CameraDistance.x, CameraDistance.y );
			}

			OrbitDistance = OrbitDistance.LerpTo( TargetOrbitDistance, Time.Delta * 10f );
		
			OrbitAngles.yaw += Input.AnalogLook.yaw;
			OrbitAngles.pitch += Input.AnalogLook.pitch;
			OrbitAngles = OrbitAngles.Normal;

			Entity.ViewAngles = OrbitAngles.WithPitch( 0f );

			OrbitAngles.pitch = OrbitAngles.pitch.Clamp( PitchClamp.x, PitchClamp.y );

			Entity.InputDirection = Input.AnalogMove;
			
			if ( Input.Down( "jump" ) )
				CameraMode = SpectatorCameraMode.Freecam;
		}
		
		if ( Input.Pressed( "attack1" ) )
			NextSpectatePlayer();
		else if (Input.Pressed("attack2" ) )
			PreviousSpectatePlayer();
		
		_lookAngles += Input.AnalogLook;
	}

	private void NextSpectatePlayer()
	{
		var currentId = (_currentFollowingPawn is null || !_currentFollowingPawn.IsValid())
			? -1
			: _currentFollowingPawn.NetworkIdent;
		
		var livingPlayers = Players.GetLiving().OrderBy(p => p.NetworkIdent ).ToList();
		var nextPlayer = livingPlayers.Where( p => p.NetworkIdent > currentId ).ToList();
		if ( nextPlayer.Any() )
		{
			_currentFollowingPawn = nextPlayer.First();
		}
		else
		{
			if ( livingPlayers.Any() )
				_currentFollowingPawn = livingPlayers.First();
			else
				CameraMode = SpectatorCameraMode.Freecam; // Fail back to FreeCam if we can't find a player
		}
	}
	
	private void PreviousSpectatePlayer()
	{
		CameraMode = SpectatorCameraMode.Spectate;
		var currentId = (_currentFollowingPawn is null || !_currentFollowingPawn.IsValid())
			? -1
			: _currentFollowingPawn.NetworkIdent;
		
		var livingPlayers = Players.GetLiving().OrderByDescending(p => p.NetworkIdent ).ToList();
		var nextPlayer = livingPlayers.Where( p => p.NetworkIdent < currentId ).ToList();
		if ( nextPlayer.Any() )
		{
			_currentFollowingPawn = nextPlayer.First();
		}
		else
		{
			if ( livingPlayers.Any() )
				_currentFollowingPawn = livingPlayers.First();
			else
				CameraMode = SpectatorCameraMode.Freecam; // Fail back to FreeCam if we can't find a player
		}
	}

	public override void Update( )
	{
		if ( CameraMode == SpectatorCameraMode.Freecam )
		{
			var mv = _moveInput.Normal * BaseMoveSpeed * RealTime.Delta * Camera.Rotation * _moveSpeed;

			if ( Camera.Rotation.Roll() > 90f || Camera.Rotation.Roll() < -90f )
				_lookAngles.pitch = _lookAngles.pitch.Clamp( -90f, 90f );

			Camera.Position += mv;
			Camera.Rotation = Rotation.From( _lookAngles );
		}
		else
		{
			var pawn = _currentFollowingPawn;
			if ( pawn is null || !pawn.IsValid() )
			{
				NextSpectatePlayer();
				return;
			}

			Camera.Position = pawn.Position;

			Camera.Position += Vector3.Up * (pawn.CollisionBounds.Center.z * pawn.Scale);
			Camera.Position += Vector3.Up * 30f;
			Camera.Rotation = Rotation.From( OrbitAngles );

			var targetPos = Camera.Position + Camera.Rotation.Backward * OrbitDistance;

			Camera.Position = targetPos;
			Camera.FieldOfView = 90f;
			Camera.FirstPersonViewer = null;

			Sound.Listener = new Transform { Position = pawn.AimRay.Position, Rotation = pawn.EyeRotation };
		}
	}
}
