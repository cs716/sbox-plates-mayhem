using System;
using System.Collections.Generic;
using System.Linq;
using PlatesGame.Entity;
using PlatesGame.State.GameStates;
using PlatesGame.util;
using Sandbox;

namespace PlatesGame.Event.Events.Arena;

public class PlateGrowEvent : BaseEvent
{
	public override int MinAffected => 1;
	public override int MaxAffected => 10;
	
	private RealTimeUntil TimeUntilScale;
	private bool ScaleCompleted;
	private const float MinGrow = .1f;
	private const float MaxGrow = .4f;
	private ICollection<PlateEntity> ImpactedPlates { get; set; } = new List<PlateEntity>();

	public override string Name => "Grow Plate";

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

		Description = $"The plate{(numPlatesImpacted != 1 ? "s" : "")} owned by {StringFormatter.FormatPlayerNames( playerNames )} will randomly grow in 5 seconds!";
		
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
			var randomScale = Random.Shared.Float( MinGrow, MaxGrow );
			plate.Grow( randomScale );
		}

		ScaleCompleted = true;
		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEventEarly = true;
		}
	}
}
