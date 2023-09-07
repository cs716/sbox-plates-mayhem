using System.Linq;
using Sandbox;

namespace PlatesGame;

public partial class CooldownState : GameState
{

	public enum CooldownFinishActions
	{
		ChangeToRandomEvent,
		ChangeToWaiting
	}

	private CooldownFinishActions CooldownFinishAction { get; }
	
	[Net] public RealTimeUntil CooldownDuration { get; set; }

	public CooldownState() : this( CooldownFinishActions.ChangeToWaiting ) { }

	public CooldownState(CooldownFinishActions nextAction)
	{
		CooldownFinishAction = nextAction;
	}

	public override void OnTick()
	{
		base.OnTick();

		if ( Game.IsClient )
			return;
		
		if ( !CooldownDuration )
			return;
		
		switch ( CooldownFinishAction )
		{
			case CooldownFinishActions.ChangeToRandomEvent:
				PlatesGame.ChangeState( new EventState() );
				break;
			case CooldownFinishActions.ChangeToWaiting:
			default:
				PlatesGame.ChangeState( new WaitingState() );
				break;
		}
	}

	public override void OnPlayerDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		if ( Game.IsClient )
			return;

		foreach (var plate in Entity.All.OfType<PlateEntity>().Where( p => p.PlateOwner == client ))
		{
			plate.IsDead = true;
		}
	}
}
