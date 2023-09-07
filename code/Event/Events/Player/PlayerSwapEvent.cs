using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public sealed class PlayerSwapEvent : BaseEvent
{
	public override EventManager.EventType EventType => EventManager.EventType.ArenaEvent;
	public override double EventWeight => 2d;
	public override int MinAffected { get; set; } = 2;

	private RealTimeUntil EventDelay;
	private List<PlayerPair> PlayerPairs { get; } = new();
	private List<string> PlayerNames { get; } = new();

	public override string Name => "Player Swap";

	public override void OnEnter()
	{
		base.OnEnter();
		
		if ( Game.IsClient )
			return;
		
		PlayerNames.Clear();
		PlayerPairs.Clear();
		
		EventDelay = Random.Shared.Float( 5f, 25f ); // Make it random so players don't just throw themselves off of their plates
		var playerCount = Entity.All.OfType<PlatesPlayer>().Count( p => p.LifeState is LifeState.Alive );
		var maxImpacted = playerCount - (playerCount % 2);
		var randomSwaps = Random.Shared.Int( MinAffected, maxImpacted );

		if ( randomSwaps % 2 != 0 ) // If the number is odd, subtract 1 to make it even without overflowing
			randomSwaps--;

		MaxAffected = randomSwaps;
		AssignPlayers();
	}

	public override void EventBegin()
	{
		base.EventBegin();
		
		foreach (var pair in PlayerPairs.Where(pair => pair.Player1?.LifeState is LifeState.Alive && pair.Player2?.LifeState is LifeState.Alive))
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

		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEvent();
		}
	}

	private void AssignPlayers()
	{
		var players = Entity.All
			.OfType<PlatesPlayer>()
			.Where( p => p.LifeState is LifeState.Alive )
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

			var player1 = pair.Player1;
			var player2 = pair.Player2;

			PlatesGame.EventDetails.AffectedEntities.Add( player1 );
			PlatesGame.EventDetails.AffectedEntities.Add( player2 );
			
			PlayerPairs.Add( pair );
			PlayerNames.Add( player1?.Client.Name );
			PlayerNames.Add( player2?.Client.Name );
		}

		PlatesGame.EventDetails.EventDescription = $"{StringFormatter.FormatPlayerNames( PlayerNames )} will be swapped at some point!";
	}

	public override void OnTick()
	{
		base.OnTick();

		if ( EventDelay && !EventBegan )
			EventBegin();
	}
}

public class PlayerPair
{
	public PlatesPlayer Player1 { get; init; }
	public PlatesPlayer Player2 { get; init; }
}
