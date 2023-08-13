using Sandbox;

namespace PlatesGame.Entity.Props;

public class LavaSpinnerEntity : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/lava_spinner.vmdl" );
		Tags.Add( "trigger" );
		EnableTouch = true;
		
		PhysicsEnabled = true;
		EnableSolidCollisions = false;
		
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		RenderColor = Color.Red;
	}

	[GameEvent.Tick.Server]
	public void ServerTick()
	{
		LocalRotation *= Rotation.FromYaw( 1f );
	}

	private RealTimeUntil NextDamageAllowed = 1;

	public override void Touch( Sandbox.Entity other )
	{
		base.Touch( other );
		
		if ( !NextDamageAllowed )
			return;
		
		other.TakeDamage( DamageInfo.Generic( 5f ) );
		Log.Info($"Touch with entity: {other.Name} ({other.Client?.Name})" );
		
		NextDamageAllowed = 1;
	}
}
