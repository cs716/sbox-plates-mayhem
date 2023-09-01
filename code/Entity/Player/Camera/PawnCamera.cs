using System;
using Sandbox;

namespace PlatesGame;

public class PawnCamera : BasePlayerCamera
{
	private static float WheelSpeed => 30f;
	private static Vector2 CameraDistance => new(125, 1000);
	private static Vector2 PitchClamp => new(-80, 80);

	private float OrbitDistance = 400f;
	private float TargetOrbitDistance = 400f;
	private Angles OrbitAngles = Angles.Zero;

	public override void Update()
	{
		var pawn = Entity;
		if ( !pawn.IsValid() )
			return;

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

	public override void BuildInput()
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
	}
}
