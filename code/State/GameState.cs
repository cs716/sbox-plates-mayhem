using System;
using System.Linq;
using PlatesGame.Entity.Player;
using PlatesGame.Event;
using PlatesGame.State.GameStates;
using PlatesGame.util;
using Sandbox;

namespace PlatesGame.State
{
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
					state.ReadyPlayers = Sandbox.Entity.All.OfType<PlatesPlayer>().Count( p => p.Alive );
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

		public virtual void OnPlayerDeath( PlatesPlayer player )
		{
			if ( Game.IsClient )
				return;
			
			var livingPlayers = Sandbox.Entity.All.OfType<PlatesPlayer>().Where( p => p.Alive );
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
		}

		public virtual float NextStateTime { get; set; }
		
		public virtual GameState NextState { get; set; }
	}
}
