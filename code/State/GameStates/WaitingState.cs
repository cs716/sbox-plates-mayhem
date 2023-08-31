using System.Linq;
using System.Threading;
using PlatesGame.Entity;
using PlatesGame.Entity.Player;
using PlatesGame.Event;
using PlatesGame.util;
using Sandbox;

namespace PlatesGame.State.GameStates;

public partial class WaitingState : GameState
{
	public override bool AllowPlayerJoins => true;
	[Net] public bool IsReady { get; set; }
	
	[Net] public int ReadyPlayers { get; set; }

	public override void OnEnter()
	{
		base.OnEnter();
		
		if (Game.IsServer)
			PlateManager.ClearBoard();

		PlateManager.CreateBoard();
		
		PlateManager.CreatePlates(Game.Clients);
		
		ReadyPlayers = Sandbox.Entity.All.OfType<PlatesPlayer>().Count( p => p.Alive );
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

		if ( Game.IsClient )
		{
			return;
		}
		
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
		if ( PlatesGame.CurrentState is WaitingState state )
		{
			return state.ReadyPlayers >= GameConfig.MinimumPlayers;
		}

		return false;
	}

	public override void OnPlayerDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		if ( Game.IsClient )
			return;

		foreach (var plate in Sandbox.Entity.All.OfType<PlateEntity>().Where( p => p.PlateOwner == client ))
		{
			plate.PlateOwner = null; 
		}
		ReadyPlayers = Sandbox.Entity.All.OfType<PlatesPlayer>().Count( p => p.Alive );
	}

	private static void StartRound()
	{
		PlatesGame.ChangeState( new CooldownState
		{
			NextStateTime = 5f,
			NextState = new EventState()
		});
	}
}
