using System.Linq;
using PlatesGame.Entity;
using PlatesGame.Event;
using Sandbox;

namespace PlatesGame.State.GameStates;

public partial class EventState : GameState
{
	[Net] public bool EndEventEarly { get; set; }
	public override float NextStateTime => 30f;

	public EventState()
	{
		if ( Game.IsClient )
			return;

		var newEvent = PlatesGame.EventManager.GetRandomEvent();
		PlatesGame.ChangeEvent(newEvent);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		PlatesGame.CurrentEvent?.OnEnter();
	}

	public override void OnExit()
	{
		base.OnExit();
		PlatesGame.CurrentEvent?.OnExit();
	}

	public override void OnTick()
	{
		base.OnTick();
		PlatesGame.CurrentEvent?.OnTick();

		if ( NextStateRealTime || EndEventEarly)
		{
			PlatesGame.CurrentEvent?.OnExit();
			PlatesGame.ChangeState( new CooldownState
			{
				NextStateTime = 5f,
				NextState = new EventState()
			});
		}
	}
	
	public override void OnPlayerDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		if ( Game.IsClient )
			return;

		foreach (var plate in Sandbox.Entity.All.OfType<PlateEntity>().Where( p => p.PlateOwner == client ))
		{
			plate.IsDead = true;
		}
	}
}
