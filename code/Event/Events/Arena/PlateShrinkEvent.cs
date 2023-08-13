using System;
using System.Linq;
using PlatesGame.State.GameStates;
using PlatesGame.util;
using Sandbox;

namespace PlatesGame.Event.Events.Arena;

public class PlateShrinkEvent : BaseEvent
{
	public override int MinAffected => 1;
	public override int MaxAffected => 10;
	
	private int PlatesImpacted;
	private RealTimeUntil TimeUntilScale;
	private bool ScaleCompleted;
	private const float MinShrink = 0.4f;
	private const float MaxShrink = 0.8f;

	public override void OnEnter()
	{
		base.OnEnter();

		if ( Game.IsClient )
			return;

		var livingPlates = PlateManager.Plates().Where( p => !p.IsDead ).OrderBy( x => Random.Shared.Double( 1, 100 ) ).ToList();
		PlatesImpacted = Random.Shared.Int( MinAffected, Math.Clamp( livingPlates.Count, MinAffected, MaxAffected ) );

		Name = "Shrink Plate";
		Description = $"{livingPlates.Count} plates will randomly shrink in 5 seconds!";
		ShortName = "Shrink Plate";

		TimeUntilScale = 5;
		ScaleCompleted = false; 
	}

	public override void OnTick()
	{
		if ( !TimeUntilScale || ScaleCompleted )
		{
			return;
		}

		var livingPlates = PlateManager.Plates().Where( p => !p.IsDead ).OrderBy( x => Random.Shared.Double( 1, 100 ) ).ToList();
		var impactCount = 0;
		foreach (var plate in livingPlates)
		{
			if ( impactCount >= PlatesImpacted )
				continue;

			impactCount++;

			var randomScale = Random.Shared.Float( MinShrink, MaxShrink );

			plate.Shrink( randomScale );
			Log.Info("Shrink: " + randomScale );
		}

		ScaleCompleted = true;
		if ( PlatesGame.State is EventState state )
		{
			state.EndEventEarly = true;
		}
	}
}
