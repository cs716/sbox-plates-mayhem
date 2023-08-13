using System.Linq;
using System.Threading;
using PlatesGame.Entity;
using PlatesGame.Event;
using PlatesGame.util;
using Sandbox;

namespace PlatesGame.State.GameStates;

public class WaitingState : GameState
{
	public override bool AllowPlayerJoins { get; init; } = true;
	private bool IsReady { get; set; }
	public override bool HandleStateChanges { get; init; }

	public override void OnEnter()
	{
		base.OnEnter();
		
		if (Game.IsServer)
			PlateManager.ClearBoard();

		PlateManager.CreateBoard();
		
		PlateManager.CreatePlates(Game.Clients);
	}

	public override void OnExit()
	{
		base.OnExit();

		if ( Game.IsClient )
			return;
		
		foreach (var plate in Sandbox.Entity.All.OfType<PlateEntity>().Where(p => p.PlateOwner == null  ))
		{
			plate.Delete();
		}
	}

	public override void OnTick()
	{
		base.OnTick();
		if ( IsReady )
		{
			if ( !CheckForMinPlayers() )
			{
				IsReady = false;
				return;
			}

			if ( NextStateRealTime )
			{
				StartRound();
			}
		}
		else
		{
			if ( CheckForMinPlayers() )
			{
				IsReady = true;
				NextStateRealTime = GameConfig.TimeBetweenRounds;
			}
		}
	}

	private static bool CheckForMinPlayers()
	{
		return Game.Clients.Count >= GameConfig.MinimumPlayers;
	}

	private static void StartRound()
	{
		PlatesGame.ChangeState( new CooldownState
		{
			AllowPlayerJoins = false,
			HandleStateChanges = true,
			NextStateTime = 5f,
			NextState = new EventState()
		});
	}
}
