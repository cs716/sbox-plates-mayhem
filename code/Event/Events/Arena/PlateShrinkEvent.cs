using System;
using System.Linq;
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
	public override void OnEnter()
	{
		base.OnEnter();

		var livingPlates = PlateManager.Plates().Where( p => !p.IsDead ).OrderBy( x => Random.Shared.Double( 1, 100 ) ).ToList();
		PlatesImpacted = Random.Shared.Int( MinAffected, Math.Clamp( livingPlates.Count, MinAffected, MaxAffected ) );

		Name = "Shrink Plate";
		Description = $"{livingPlates.Count} plates will randomly shrink in 5 seconds!";
		ShortName = "Shrink Plate";

		TimeUntilScale = 5;
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

			var randomScale = Random.Shared.Int( 10, 90 );
			if ( randomScale % 2 == 1 ) // Make it an even number
				randomScale++;

			plate.Shrink( randomScale );
		}

		ScaleCompleted = true;
	}
}
