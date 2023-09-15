using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public class PlayerInfectionEvent : BaseEvent
{
	public override int MinAffected => 1;
	public override int MaxAffected => 5;
	public override float EventBeginDelay => 10f;
	public override string Name => "The Plague";

	public override void OnInvoked()
	{
		base.OnInvoked();

		if ( Game.IsClient )
			return;

		var players = Players.GetLiving().OrderBy( x => Random.Shared.Double( 1, 100 ) ).ToList();
		var numPlatesImpacted = Random.Shared.Int( MinAffected, Math.Clamp( players.Count, MinAffected, MaxAffected ) );
		List<string> playerNames = new();
		for ( var i = 0; i < numPlatesImpacted; i++ )
		{
			PlatesGame.EventDetails.AffectedEntities.Add( players[i] );
			playerNames.Add( players[i].Client.Name );
		}

		PlatesGame.EventDetails.EventDescription = $"A plague will infect {StringFormatter.FormatPlayerNames( playerNames )}! Avoid them if you like your health!";
	}

	public override void OnStart()
	{
		base.OnStart();
		
		foreach (var player in PlatesGame.EventDetails.AffectedEntities.OfType<PlatesPlayer>().Where(p => p.LifeState == LifeState.Alive  ))
		{
			player.AddModifier( PlatesPlayer.PlayerModifier.Poisoned );
		}
		
		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEvent();
		}
	}
}
