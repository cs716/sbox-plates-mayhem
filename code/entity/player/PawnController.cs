
using System;
using Sandbox;

namespace Pl8Mayhem;

public partial class PawnController : EntityComponent<PlatesPlayer>
{
	protected PlatesPlayer Player => Entity;

    [ClientInput] public Vector3 WishDirection { get; set; } = Vector3.Zero;

    [ClientInput] public bool IsRunning { get; set; } = false;

    [Net] public float WalkSpeed { get; set; } = 80f;

    [Net] public float RunSpeed { get; set; } = 150f;

    [Net] public float AccelerationSpeed { get; set; } = 200f; // Units per second (Ex. 200f means that after 1 second you've reached 200f speed)

    [Net] public float WishSpeed { get; private set; } = 0f;

    public Vector3 WishVelocity => WishDirection.Normal * WishSpeed;

    public Rotation WishRotation => Rotation.LookAt( WishDirection.WithZ( 0 ), Vector3.Up );

    public float CollisionWidth { get; set; } = 20f;

    public float CollisionHeight { get; set; } = 40f;

    public BBox CollisionBox => new( new Vector3( -CollisionWidth / 2f, -CollisionWidth / 2f, 0f ), new Vector3( CollisionWidth / 2f, CollisionWidth / 2f, CollisionHeight ) );

    public float StepSize => 16f;

    public float MaxWalkableAngle => 60f;

    public virtual void Simulate()
    {
        if ( WishDirection != Vector3.Zero )
            WishSpeed = Math.Clamp( WishSpeed + AccelerationSpeed * Time.Delta, 0f, IsRunning ? RunSpeed : WalkSpeed );
        else
            WishSpeed = 0f;

        Player.Velocity = Vector3.Lerp( Player.Velocity, WishVelocity, 15f * Time.Delta ) // Smooth horizontal movement
            .WithZ( Player.Velocity.z ); // Don't smooth vertical movement

        // Apply gravity
        if ( Player.GroundEntity == null )
            Player.Velocity -= Vector3.Down * Game.PhysicsWorld.Gravity * Time.Delta;

        // Get and set new position according to velocity
        var helper = new MoveHelper( Player.Position, Player.Velocity ) { MaxStandableAngle = MaxWalkableAngle };

        helper.Trace = helper.Trace
                        .Size( CollisionBox.Mins, CollisionBox.Maxs )
                        .Ignore( Player );

        helper.TryUnstuck();
        helper.TryMoveWithStep( Time.Delta, StepSize );

        Player.Position = helper.Position;
        Player.Velocity = helper.Velocity;

        // GroundEntity check.
        if ( Player.Velocity.z <= StepSize )
        {
            var tr = helper.TraceDirection( Vector3.Down );

            Player.GroundEntity = tr.Entity;

            if ( Player.GroundEntity != null )
            {
                Player.Position += tr.Distance * Vector3.Down; // Snap to ground

                if ( Player.Velocity.z < 0.0f )
                    Player.Velocity = Player.Velocity.WithZ( 0 );
            }
        }
        else
            Player.GroundEntity = null;

        if ( WishDirection != Vector3.Zero )
            Player.Rotation = Rotation.Lerp( Player.Rotation, WishRotation, Time.Delta * 10f );

        SimulateAnimator();
    }

    public void BuildInput()
    {
        IsRunning = Input.Down( "Run" );
    }

    protected void SimulateAnimator()
    {
        CitizenAnimationHelper animHelper = new CitizenAnimationHelper( Player );

        animHelper.WithWishVelocity( WishVelocity );
        animHelper.WithVelocity( Player.Velocity );
        animHelper.WithLookAt( Player.Position + Player.Rotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
        animHelper.AimAngle = Player.Rotation;
        animHelper.IsGrounded = Player.GroundEntity != null;
        animHelper.MoveStyle = Input.Down( "Run" ) ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
    }
}
