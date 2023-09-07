using System;
using Sandbox;

namespace PlatesGame;

public partial class RoundEndState : GameState
{
	[Net] public PlatesPlayer RoundWinner { get; set; }
	[Net] public RoundEndReason Outcome { get; set; }

	private RealTimeUntil _nextRoundTimer = 10f;

	public enum RoundEndReason
	{
		OnePlayerAlive,
		EverybodyLeft,
		EverybodyDied,
		Unknown
	}

	public RoundEndState() : this( RoundEndReason.Unknown ) { }

	public RoundEndState( RoundEndReason reason, PlatesPlayer winner = null )
	{
		Outcome = reason; 
		
		if (winner != null)
			RoundWinner = winner; 
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Log.Info($"Winner: {RoundWinner?.Client?.Name}!"  );
	}

	public override void OnTick()
	{
		base.OnTick();

		if ( Game.IsClient )
			return;

		if ( _nextRoundTimer )
			PlatesGame.ChangeState( new WaitingState() );
	}
}
