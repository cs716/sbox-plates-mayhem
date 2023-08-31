
using System;
using System.Collections.Generic;
using PlatesGame.State.GameStates;
using Sandbox;

namespace PlatesGame.Entity.Player;

public class PawnController : EntityComponent<PlatesPlayer>
{
	private static int StepSize => 24;
	private static int GroundAngle => 45;
	private static int JumpSpeed => 400;
	public float Gravity { get; set; } = 800f;

	private readonly HashSet<string> ControllerEvents = new( StringComparer.OrdinalIgnoreCase );

	private bool Grounded => Entity.GroundEntity.IsValid();

	public void Simulate( IClient cl )
	{
		ControllerEvents.Clear();

		var movement = (PlatesGame.CurrentState is WaitingState || Entity.Client.IsBot) ? Vector3.Zero : Input.AnalogMove;
		var angles = Entity.ViewAngles.WithPitch( 0 );
		var moveVector = Rotation.From( angles ) * movement * 320f;
		var groundEntity = CheckForGround();

		/*if ( Game.IsClient )
		{
			DebugOverlay.ScreenText($"Position: {Entity.Position}", (int)PlatesGame.DebugTextLocations.PlayerData);
			DebugOverlay.ScreenText($"Alive: {Entity.Alive}", (int)PlatesGame.DebugTextLocations.PlayerData + 1);
			DebugOverlay.ScreenText( $"Health: {Entity.Health}", (int)PlatesGame.DebugTextLocations.PlayerData + 2 );
		}*/
		
		if ( Input.Pressed( "jump" ) )
		{
			DoJump();
		}

		if ( groundEntity.IsValid() )
		{
			if ( !Grounded )
			{
				Entity.Velocity = Entity.Velocity.WithZ( 0 );
				AddEvent( "grounded" );
			}

			Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, 180.0f * ( Input.Down( "run" ) ? 1.5f : 1f ), 7.5f );
			Entity.Velocity = ApplyFriction( Entity.Velocity, 4.0f );
		}
		else
		{
			Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, 100, 20f );
			Entity.Velocity += Vector3.Down * Gravity * Time.Delta;
		}

		var mh = new MoveHelper( Entity.Position, Entity.Velocity );
		mh.Trace = mh.Trace.WithAnyTags("solid").Size( Entity.Hull ).Ignore( Entity );

		if ( mh.TryMoveWithStep( Time.Delta, StepSize ) > 0 )
		{
			if ( Grounded )
			{
				mh.Position = StayOnGround( mh.Position );
			}
			Entity.Position = mh.Position;
			Entity.Velocity = mh.Velocity;
		}

		Entity.GroundEntity = groundEntity;
	}

	void DoJump()
	{
		if ( Grounded )
		{
			Entity.Velocity = ApplyJump( Entity.Velocity, "jump" );
		}
	}

	Sandbox.Entity CheckForGround()
	{
		if ( Entity.Velocity.z > 100f )
			return null;

		var trace = Entity.TraceBBox( Entity.Position, Entity.Position + Vector3.Down, 2f );

		if ( !trace.Hit )
			return null;

		return trace.Normal.Angle( Vector3.Up ) > GroundAngle ? null : trace.Entity;
	}

	Vector3 ApplyFriction( Vector3 input, float frictionAmount )
	{
		const float stopSpeed = 100.0f;

		var speed = input.Length;
		if ( speed < 0.1f ) return input;

		// Bleed off some speed, but if we have less than the bleed
		// threshold, bleed the threshold amount.
		var control = (speed < stopSpeed) ? stopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		var newSpeed = speed - drop;
		if ( newSpeed < 0 ) newSpeed = 0;
		if ( newSpeed.AlmostEqual(speed) ) return input;

		newSpeed /= speed;
		input *= newSpeed;

		return input;
	}

	Vector3 Accelerate( Vector3 input, Vector3 wishDir, float wishSpeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishSpeed > speedLimit )
			wishSpeed = speedLimit;

		var currentSpeed = input.Dot( wishDir );
		var addSpeed = wishSpeed - currentSpeed;

		if ( addSpeed <= 0 )
			return input;

		var accelSpeed = acceleration * Time.Delta * wishSpeed;

		if ( accelSpeed > addSpeed )
			accelSpeed = addSpeed;

		input += wishDir * accelSpeed;

		return input;
	}

	Vector3 ApplyJump( Vector3 input, string jumpType )
	{
		AddEvent( jumpType );

		return input + Vector3.Up * JumpSpeed;
	}

	Vector3 StayOnGround( Vector3 position )
	{
		var start = position + Vector3.Up * 2;
		var end = position + Vector3.Down * StepSize;

		// See how far up we can go without getting stuck
		var trace = Entity.TraceBBox( position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = Entity.TraceBBox( start, end );

		switch (trace.Fraction)
		{
			case <= 0:
				return position;
			case >= 1:
				return position;
		}

		if ( trace.StartedSolid ) return position;
		return Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ? position : trace.EndPosition;
	}

	public bool HasEvent( string eventName )
	{
		return ControllerEvents.Contains( eventName );
	}

	void AddEvent( string eventName )
	{
		if ( HasEvent( eventName ) )
			return;

		ControllerEvents.Add( eventName );
	}
}
