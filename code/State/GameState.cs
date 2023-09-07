using System.Linq;
using Sandbox;

namespace PlatesGame;

public partial class GameState : BaseNetworkable
{
	public virtual void OnEnter()
	{
		Log.Info( "OnEnter - " + ClassName );
	}
	public virtual void OnExit() { 
		Log.Info( "OnExit - " + ClassName );
	}

	public virtual void OnPlayerConnect( IClient client ) {}

	public virtual void OnPlayerDisconnect( IClient cl, NetworkDisconnectionReason reason ) { }

	public virtual void OnTick()
	{
	}

	public virtual void OnPlayerDeath( PlatesPlayer player )
	{
		if ( Game.IsClient )
			return;
			
		var livingPlayers = Entity.All.OfType<PlatesPlayer>().Where( p => p.LifeState is LifeState.Alive );
		var platesPlayers = livingPlayers as PlatesPlayer[] ?? livingPlayers.ToArray();
		if ( !platesPlayers.Any() )
		{
			PlatesGame.ChangeState( new RoundEndState(RoundEndState.RoundEndReason.EverybodyDied));
		} else if ( platesPlayers.Length == 1 )
		{
			var winner = platesPlayers.First();
			PlatesGame.ChangeState( new RoundEndState(RoundEndState.RoundEndReason.OnePlayerAlive, winner));
		}
	}
}
