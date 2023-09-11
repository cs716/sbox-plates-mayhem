using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public class PlateShrinkEvent : BaseEvent
{
	public override int MinAffected => 1;
	public override int MaxAffected => 10;
	
	private const float MinShrink = 0.1f;
	
	private const float MaxShrink = 0.3f;
	public override float EventBeginDelay => 10f;
	public override string Name => "Shrink Plate";

	public override void OnInvoked()
	{
		base.OnInvoked();

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

		PlatesGame.EventDetails.EventDescription = $"The plate{(numPlatesImpacted != 1 ? "s" : "")} owned by {StringFormatter.FormatPlayerNames( playerNames )} will randomly shrink in 5 seconds!";
	}

	public override void OnStart()
	{
		base.OnStart();
		
		foreach (var plate in PlatesGame.EventDetails.AffectedEntities.OfType<PlateEntity>().Where(p => !p.IsDead  ))
		{
			var randomScale = Random.Shared.Float( MinShrink, MaxShrink );
			plate.Shrink( randomScale );
		}
		
		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEvent();
		}
	}
}
