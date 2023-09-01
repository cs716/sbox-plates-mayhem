using System.Linq;
using Sandbox;

namespace PlatesGame;

public partial class GameState : BaseNetworkable
{
	public virtual bool AllowPlayerJoins => false;
	public virtual bool HandleStateChanges => false;
		
	[Net] public RealTimeUntil NextStateRealTime { get; set; }

	public virtual void OnEnter()
	{
		NextStateRealTime = NextStateTime;
		Log.Info( "OnEnter - " + ClassName );
	}
	public virtual void OnExit() { 
		Log.Info( "OnExit - " + ClassName );
			
	}

	public virtual void OnPlayerConnect( IClient client )
	{
		if ( Game.IsClient )
			return;
			
		if ( AllowPlayerJoins )
		{
			PlateManager.FindAndAssignPlate( client );
			if ( this is WaitingState state )
			{
				state.ReadyPlayers = Entity.All.OfType<PlatesPlayer>().Count( p => p.LifeState is LifeState.Alive );
			}
		}
	}

	public virtual void OnPlayerDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
			
	}

	public virtual void OnTick()
	{
		if (Game.IsClient) {
			//DebugOverlay.ScreenText($"Current State {ClassName}", (int)PlatesGame.DebugTextLocations.StateData );
			//DebugOverlay.ScreenText($"Time: {NextStateRealTime.Relative.CeilToInt()}", (int)PlatesGame.DebugTextLocations.StateData + 1 );
			return;
		}
			
		if ( Game.IsServer && NextStateRealTime && HandleStateChanges)
		{
			PlatesGame.ChangeState( NextState );
		}
	}

	public virtual bool OnPlayerDeath( PlatesPlayer player )
	{
		if ( Game.IsClient )
			return true;
			
		var livingPlayers = Entity.All.OfType<PlatesPlayer>().Where( p => p.LifeState is LifeState.Alive );
		var platesPlayers = livingPlayers as PlatesPlayer[] ?? livingPlayers.ToArray();
		if ( !platesPlayers.Any() )
		{
			PlatesGame.ChangeState( new RoundEndState
			{
				WinnerName = "Nobody",
				WinnerNetId = -1,
				NextStateRealTime = 5f
			});
		} else if ( platesPlayers.Length == 1 )
		{
			var winner = platesPlayers.First();
			PlatesGame.ChangeState( new RoundEndState
			{
				WinnerName = winner.Client.Name,
				WinnerNetId = winner.NetworkIdent,
				NextStateRealTime = 5f
			});
		}

		return true;
	}

	public virtual float NextStateTime { get; set; }
		
	public virtual GameState NextState { get; set; }
}
