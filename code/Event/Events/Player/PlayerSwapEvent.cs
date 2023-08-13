using System;
using System.Collections.Generic;
using System.Linq;
using PlatesGame.Entity.Player;
using PlatesGame.State.GameStates;
using PlatesGame.util;
using Sandbox;

namespace PlatesGame.Event.Events.Player;

public sealed class PlayerSwapEvent : BaseEvent
{
	public override EventManager.EventType EventType => EventManager.EventType.ArenaEvent;
	public override double EventWeight => 2d;
	public override int MinAffected { get; set; } = 2;

	private RealTimeUntil EventDelay;
	private bool EventOperationsComplete;
	private List<PlayerPair> PlayerPairs { get; } = new();
	private List<string> PlayerNames { get; } = new();

	public override void OnEnter()
	{
		base.OnEnter();
		Name = "Player Swap";
		ShortName = "Player Swap";
		
		if ( Game.IsClient )
			return;
		
		PlayerNames.Clear();
		PlayerPairs.Clear();
		EventOperationsComplete = false;
		
		EventDelay = Random.Shared.Float( 5f, 25f ); // Make it random so players don't just throw themselves off of their plates
		var playerCount = Sandbox.Entity.All.OfType<PlatesPlayer>().Count( p => p.Alive );
		var maxImpacted = playerCount - (playerCount % 2);
		var randomSwaps = Random.Shared.Int( MinAffected, maxImpacted );

		if ( randomSwaps % 2 != 0 ) // If the number is odd, subtract 1 to make it even without overflowing
			randomSwaps--;

		MaxAffected = randomSwaps;
		//Description = $"{MaxAffected} players will swap positions at some point";
		AssignPlayers();
	}

	private void PerformSwaps()
	{
		foreach (var pair in PlayerPairs.Where(pair => pair.Player1?.Alive == true && pair.Player2?.Alive == true))
		{
			var p1Pos = pair.Player1.Position;
			var p2Pos = pair.Player2.Position;
			var p1Vel = pair.Player1.Velocity;
			var p2Vel = pair.Player2.Velocity;

			pair.Player1.Position = p2Pos;
			pair.Player1.Velocity = p2Vel;

			pair.Player2.Position = p1Pos;
			pair.Player2.Velocity = p1Vel;
		}

		if ( PlatesGame.State is EventState state )
		{
			state.EndEventEarly = true;
		}
	}

	private void AssignPlayers()
	{
		var players = Sandbox.Entity.All
			.OfType<PlatesPlayer>()
			.Where( p => p.Alive )
			.OrderBy( x => Random.Shared.Double( 1, 100 ) )
			.ToList();
		
		for ( var i = 0; i < MaxAffected; i += 2 )
		{
			if ( i + 1 >= MaxAffected )
			{
				return;
			}

			var pair = new PlayerPair
			{
				Player1 = players[i],
				Player2 = players[i + 1]
			};
			PlayerPairs.Add( pair );
			PlayerNames.Add( players[i]?.Client.Name );
			PlayerNames.Add( players[i+1]?.Client.Name );
		}

		Description = $"{StringFormatter.FormatPlayerNames( PlayerNames )} will be swapped at some point!";
	}

	public override void OnTick()
	{
		base.OnTick();

		if ( !EventDelay || EventOperationsComplete )
		{
			return;
		}

		EventOperationsComplete = true;
		if (Game.IsServer)
			PerformSwaps();
	}
}

public class PlayerPair
{
	public PlatesPlayer Player1 { get; set; }
	public PlatesPlayer Player2 { get; set; }
}
