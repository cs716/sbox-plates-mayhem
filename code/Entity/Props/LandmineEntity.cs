using PlatesGame.Entity.Player;
using PlatesGame.util;
using Sandbox;

namespace PlatesGame.Entity.Props;

public class LandmineEntity : Prop
{
	private RealTimeSince TimeSinceCreation = 0f;
	private RealTimeUntil TimeUntilDetonation = 0f;
	private bool IsDetonating;
	private bool IsDetonated;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/landmine.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		Name = "Landmine";
		RenderColor = RenderColor.WithAlpha( 0f );
		EnableAllCollisions = false;
		IsDetonated = false;

		var trigger = new BaseTriggerEntity() { Holder = this, Enabled = true };
		trigger.SetTriggerRadius( 8f );
		trigger.Trigger = other =>
		{
			if ( IsDetonating || !IsValid)
				return true;
			
			if ( !other.Tags.Has("damagingTrigger" ) && ( TimeSinceCreation < 3f || !other.IsValid() || 
				other is PlateEntity ||
			    other is LandmineEntity ||
			    other is BaseTriggerEntity ))
			{
				return true;
			}

			IsDetonating = true;
			TimeUntilDetonation = 0.8f;
			PlaySound( "sounds/mine.detonate.sound" );
			return true;
		};

		trigger.Spawn();
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
		if ( IsDetonating || IsDetonated )
			return;
		
		IsDetonating = true;
        TimeUntilDetonation = 0.8f;
        PlaySound( "sounds/mine.detonate.sound" );
	}

	[GameEvent.Tick]
	public void Tick()
	{
		//DebugOverlay.Text( $"Detonating: {IsDetonating}, Detonated: {IsDetonated}, TimeUntil: {TimeUntilDetonation}",
			//Position.WithZ( 5 ), Color.Green, 0f );
		if ( RenderColor.a < 1f )
			RenderColor = RenderColor.WithAlpha( RenderColor.a + 0.005f );

		if (IsDetonating)
			RenderColor = Color.Red;

		if ( !IsDetonating || !TimeUntilDetonation || !Game.IsServer || IsDetonated) 
			return;
		
		IsDetonated = true; 
		ArenaHelper.Explosion( this, Position, 80f, 200, 2.0f, true );
		DeleteAsync(1f);
	}
}
