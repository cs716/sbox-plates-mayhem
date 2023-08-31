using System;
using System.Collections.Generic;
using System.Linq;
using PlatesGame.Entity;
using PlatesGame.State.GameStates;
using PlatesGame.util;
using Sandbox;

namespace PlatesGame.Event.Events.Arena;

public class PlateShrinkEvent : BaseEvent
{
	public override int MinAffected => 1;
	public override int MaxAffected => 10;
	
	private RealTimeUntil TimeUntilScale;
	private bool ScaleCompleted;
	private const float MinShrink = 0.1f;
	private const float MaxShrink = 0.3f;

	public override string Name => "Shrink Plate";

	private ICollection<PlateEntity> ImpactedPlates { get; set; } = new List<PlateEntity>();

	public override void OnEnter()
	{
		base.OnEnter();

		if ( Game.IsClient )
			return;

		ImpactedPlates.Clear();

		var livingPlates = PlateManager.Plates().Where( p => !p.IsDead ).OrderBy( x => Random.Shared.Double( 1, 100 ) ).ToList();
		var numPlatesImpacted = Random.Shared.Int( MinAffected, Math.Clamp( livingPlates.Count, MinAffected, MaxAffected ) );
		List<string> playerNames = new();
		for ( var i = 0; i < numPlatesImpacted; i++ )
		{
			ImpactedPlates.Add(livingPlates[i]);
			livingPlates[i].WasImpacted = true;
			playerNames.Add( livingPlates[i].OwnerName );
		}

		Description = $"The plate{(numPlatesImpacted != 1 ? "s" : "")} owned by {StringFormatter.FormatPlayerNames( playerNames )} will randomly shrink in 5 seconds!";
		
		TimeUntilScale = 5;
		ScaleCompleted = false; 
	}

	public override void OnTick()
	{
		if ( !TimeUntilScale || ScaleCompleted )
		{
			return;
		}
		
		foreach (var plate in ImpactedPlates)
		{
			var randomScale = Random.Shared.Float( MinShrink, MaxShrink );
			plate.Shrink( randomScale );
		}

		ScaleCompleted = true;
		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEventEarly = true;
		}
	}
}
