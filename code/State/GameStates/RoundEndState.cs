using Sandbox;

namespace PlatesGame.State.GameStates;

public partial class RoundEndState : CooldownState
{
	[Net] public string WinnerName { get; set; }
	[Net] public int WinnerNetId { get; set; }

	public override float NextStateTime { get; set; }

	public override void OnEnter()
	{
		NextStateTime = GameConfig.WinnerScreenTime;
		
		base.OnEnter();
		
		Log.Info($"Winner: {WinnerName} ({WinnerNetId})!"  );
		NextState = new WaitingState();
	}
}
