using System;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public class FallingProp : Prop
{
	public FallingProp()
	{
		EnableTouch = true;
		EnableAllCollisions = true;
		Invulnerable = 100f;
		Tags.Add( "trigger", "solid" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
	}

	public float BlastRadius { get; set; } = 80f;
	public float BaseDamage { get; set; } = 80f;

	public float BaseForce { get; set; } = 800f;

	private bool Exploded; 

	public override void Touch( Entity other )
	{
		base.Touch( other );

		if ( Exploded )
			return;
		
		var originPos = Position;
		ArenaHelper.Explosion( this, originPos, BlastRadius, BaseDamage, BaseForce, true );
		Exploded = true;
		if (Game.IsServer)
		{
			EnableDrawing = false;
			DeleteAsync( 1f );
		}
	}

	[GameEvent.Tick.Server]
	public void OnTick()
	{
		if ( Position.z < -1000f )
		{
			DeleteAsync( 1f );
		}
	}

	public override void OnKilled()
	{
		if ( !IsValid )
			return;
		
		 // Don't create effects - The barrel does this for us
		base.OnKilled();
	}
}
