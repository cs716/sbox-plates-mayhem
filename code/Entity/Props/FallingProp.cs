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
	private float BlastRadius { get; set; } = 80f;
	private float BaseDamage { get; set; } = 80f;

	private float BaseForce { get; set; } = 800f;

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

	public override void OnKilled()
	{
		if ( !IsValid )
			return;
		
		 // Don't create effects - The barrel does this for us
		base.OnKilled();
	}
}
