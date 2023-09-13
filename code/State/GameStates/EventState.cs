using System.Linq;
using System.Net.Mail;
using Sandbox;

namespace PlatesGame;

public partial class EventState : GameState
{
	[Net] public RealTimeUntil EventDuration { get; set; }

	private bool _durationBasedEvent = false; 

	public EventState()
	{
		if ( Game.IsClient )
			return;

		var newEvent = PlatesGame.EventManager.GetRandomEvent();
		PlatesGame.ChangeEvent(newEvent);
	}

	public void EndEvent()
	{
		_durationBasedEvent = true;
		EventDuration = 0f;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		PlatesGame.CurrentEvent?.OnInvoked();
		
		if ( Game.IsClient )
			return;

		if ( PlatesGame.CurrentEvent?.EventDuration >= 0f )
		{
			EventDuration = PlatesGame.CurrentEvent.EventDuration;
			_durationBasedEvent = true;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		PlatesGame.CurrentEvent?.OnExit();
	}

	public override void OnTick()
	{
		base.OnTick();
		
		PlatesGame.CurrentEvent?.Tick();

		if ( Game.IsClient )
			return;

		if ( !_durationBasedEvent || !EventDuration )
			return;
		
		PlatesGame.CurrentEvent?.OnExit();
		PlatesGame.ChangeState( new CooldownState(CooldownState.CooldownFinishActions.ChangeToRandomEvent) { CooldownDuration = GameConfig.TimeBetweenRounds });
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
