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
		public virtual bool AllowPlayerJoins { get; init; } = false;

		[Net] public BaseEvent CurrentEvent { get; set; }

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
			if ( AllowPlayerJoins )
			{
				PlateManager.FindAndAssignPlate( client );
			}
		}

		public virtual void OnTick()
		{
			if (Game.IsClient) {
				DebugOverlay.ScreenText($"Current State {ClassName}", (int)PlatesGame.DebugTextLocations.StateData );
				DebugOverlay.ScreenText($"Next {NextState?.ClassName}", (int)PlatesGame.DebugTextLocations.StateData + 1 );
				DebugOverlay.ScreenText($"Time: {Math.Floor(NextStateRealTime.Passed)}/{NextStateTime}", (int)PlatesGame.DebugTextLocations.StateData + 2 );
			}
			
			if ( NextStateRealTime && HandleStateChanges)
			{
				PlatesGame.ChangeState( NextState );
			}
		}

		public virtual void OnPlayerDeath( PlatesPlayer player )
		{
			var livingPlayers = Sandbox.Entity.All.OfType<PlatesPlayer>().Where( p => p.Alive );
			var platesPlayers = livingPlayers as PlatesPlayer[] ?? livingPlayers.ToArray();
			if ( !platesPlayers.Any() )
			{
				PlatesGame.ChangeState( new RoundEndState
				{
					WinnerName = "Nobody",
					WinnerNetId = -1,
					HandleStateChanges = true,
					NextStateRealTime = 5f
				});
			} else if ( platesPlayers.Length == 1 )
			{
				var winner = platesPlayers.First();
				PlatesGame.ChangeState( new RoundEndState
				{
					WinnerName = winner.Client.Name,
					WinnerNetId = winner.NetworkIdent,
					HandleStateChanges = true,
					NextStateRealTime = 5f
				});
			}
		}

		public virtual float NextStateTime { get; set; }
		public virtual RealTimeUntil NextStateRealTime { get; set; }

		public virtual bool HandleStateChanges { get; init; } = false;
		
		public virtual GameState NextState { get; set; }
	}
}
