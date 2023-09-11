using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public class PoisonPlates : BaseEvent
{
	public override int MinAffected => 1;
	public override int MaxAffected => 10;
	public override float EventBeginDelay => 10f;
	public override string Name => "Poison Plates";

	public override void OnEnter()
	{
		base.OnEnter();

		if ( Game.IsClient )
			return;

		var livingPlates = PlateManager.Plates().Where( p => !p.IsDead ).OrderBy( x => Random.Shared.Double( 1, 100 ) ).ToList();
		var numPlatesImpacted = Random.Shared.Int( MinAffected, Math.Clamp( livingPlates.Count, MinAffected, MaxAffected ) );
		List<string> playerNames = new();
		for ( var i = 0; i < numPlatesImpacted; i++ )
		{
			PlatesGame.EventDetails.AffectedEntities.Add( livingPlates[i] );
			playerNames.Add( livingPlates[i].OwnerName );
		}

		PlatesGame.EventDetails.EventDescription = $"The plate{(numPlatesImpacted != 1 ? "s" : "")} owned by {StringFormatter.FormatPlayerNames( playerNames )} will be poisoned!";
	}

	public override void EventBegin()
	{
		base.EventBegin();
		
		foreach (var plate in PlatesGame.EventDetails.AffectedEntities.OfType<PlateEntity>().Where(p => !p.IsDead  ))
		{
			plate.AddModifier(PlateEntity.PlateModifier.Poison);
		}
		
		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEvent();
		}
	}
}
