using System;
using Sandbox;

namespace PlatesGame;

public class BaseTriggerEntity : ModelEntity
{
	public Func<Entity, bool> Trigger;
	public Entity Holder;
	public bool Enabled = false;

	public override void Spawn()
	{
		base.Spawn();

		// Set the default size
		SetTriggerRadius( 16 );

		// Client doesn't need to know about this
		Transmit = TransmitType.Never;
	}

	public void SetTriggerRadius( float radius )
	{
		SetupPhysicsFromCapsule(PhysicsMotionType.Keyframed, new Capsule(Vector3.Zero, Vector3.One * 0.1f, radius));
		Tags.Add("trigger");
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
		info.Attacker.Tags.Add("damagingTrigger"  );
		Touch( info.Attacker );
	}

	[GameEvent.Tick.Server]
	public void ServerTick()
	{
		if(Holder.IsValid()) 
			Position = Holder.Position;
		else 
			Delete();
	}

	public override void Touch(Entity other)
	{
		base.Touch(other);
		
		if(Enabled && Trigger != null) 
			Trigger(other);
	}
}
