using System.Linq;
using Sandbox;

namespace PlatesGame;

public partial class WaitingState : GameState
{
	[Net] public bool IsReady { get; set; }
	
	[Net] public int ReadyPlayers { get; set; }

	[Net] public RealTimeUntil StartRoundTime { get; set; }

	public override void OnEnter()
	{
		base.OnEnter();

		if ( Game.IsServer )
		{
			PlateManager.ClearBoard();
			PlatesGame.Instance.ArenaGravity = GameConfig.DefaultGravity;
		}

		PlateManager.CreateBoard();
		
		PlateManager.CreatePlates(Game.Clients);
		
		ReadyPlayers = Players.GetLiving().Count();
	}

	public override void OnExit()
	{
		base.OnExit();

		if ( Game.IsClient )
			return;
		
		foreach (var plate in Entity.All.OfType<PlateEntity>().Where(p => p.PlateOwner == null  ))
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

			if ( StartRoundTime )
			{
				StartRound();
			}
		}
		else
		{
			if ( CheckForMinPlayers() )
			{
				IsReady = true;
				StartRoundTime = GameConfig.TimeBetweenRounds;
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

	public override void OnPlayerConnect( IClient client )
	{
		base.OnPlayerConnect( client );

		if ( Game.IsClient )
			return;
		
		PlateManager.FindAndAssignPlate( client );
		ReadyPlayers = Players.GetLiving().Count();
	}

	public override void OnPlayerDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		if ( Game.IsClient )
			return;

		foreach (var plate in Entity.All.OfType<PlateEntity>().Where( p => p.PlateOwner == client ))
		{
			plate.PlateOwner = null; 
		}
		ReadyPlayers = Players.GetLiving().Count();
	}

	private static void StartRound()
	{
		foreach (var platesPlayer in Players.GetLiving())
		{
			PlateManager.ReturnPlayerToPlate( platesPlayer );
		}

		PlatesGame.ChangeState( new CooldownState(CooldownState.CooldownFinishActions.ChangeToRandomEvent)
		{
			CooldownDuration = 5f
		});
	}
}
