using System;
using Sandbox;

namespace PlatesGame.util;

public class ArenaHelper
{
	public static void Explosion( Sandbox.Entity owner, Vector3 position, float radius, float damage, float forceScale, bool createEffects)
	{
		// Effects
		if ( createEffects )
		{
			Sound.FromWorld( "rust_pumpshotgun.shootdouble", position );
			Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", position );
		}
		
		//DebugOverlay.Sphere(position, radius, Color.Red, 2f);
		
		// Damage, etc
		var overlaps = Sandbox.Entity.FindInSphere( position, radius );

		foreach ( var overlap in overlaps )
		{
			Log.Info( overlap );
			if ( overlap is not ModelEntity ent || !ent.IsValid() )
				continue;

			if ( ent.LifeState != LifeState.Alive )
				continue;

			if ( !ent.PhysicsBody.IsValid() )
				continue;

			if ( ent.IsWorld )
				continue;

			var targetPos = ent.PhysicsBody.MassCenter;

			var dist = Vector3.DistanceBetween( position, targetPos );
			if ( dist > radius )
				continue;

			var tr = Trace.Ray( position, targetPos )
				.StaticOnly()
				.Run();

			if ( tr.Fraction < 0.98f )
				continue;

			var distanceMul = 1.0f - Math.Clamp( dist / radius, 0.0f, 1.0f );
			var dmg = damage * distanceMul;
			var force = (forceScale * distanceMul) * ent.PhysicsBody.Mass;
			var forceDir = (targetPos - position).Normal;
			var damageInfo = DamageInfo.FromExplosion( position, forceDir * force, dmg )
				.WithAttacker( owner );

			ent.TakeDamage( damageInfo );
		}
	}
}
