using System;
using System.Linq;
using PlatesGame.Entity.Player;
using PlatesGame.util;
using Sandbox;

namespace PlatesGame.Entity.Props;

public class BarrelEntity : Prop
{

	public BarrelEntity()
	{
		EnableTouch = true;
		EnableAllCollisions = true;
		Invulnerable = 100f;
		Tags.Add( "trigger" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
	}
	private float BlastRadius { get; set; } = 80f;
	private float BaseDamage { get; set; } = 80f;

	private float BaseForce { get; set; } = 800f;

	private bool Exploded; 

	public override void Touch( Sandbox.Entity other )
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
		/*foreach (var ent in All.OfType<PlatesPlayer>().Where(p => p.Alive && Vector3.DistanceBetween(originPos, p.Position) < BlastRadius ))
		{
			
			var entityPos = ent.Position;
			var distance = Vector3.DistanceBetween( originPos, entityPos );
			Log.Info(distance  );
			if ( distance > BlastRadius )
				continue;

			var distanceMult = 1.0f - Math.Clamp( distance / BlastRadius, 0.0f, 1.0f );
			var adjustedDamage = BaseDamage * distanceMult;
			var adjustedForce = BaseForce * distanceMult;
			var forceDirection = (entityPos - originPos).Normal;

			ent.TakeDamage(DamageInfo.FromExplosion( originPos, forceDirection * adjustedForce, adjustedDamage ).WithAttacker(this));
			Log.Info($"TakeDamage on {ent.Name} ({ent.Client?.Name}) - {adjustedDamage}"  );
		}*/
		base.OnKilled();
	}
}
