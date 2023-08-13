using PlatesGame.Event;
using Sandbox;

namespace PlatesGame.State.GameStates;

public partial class EventState : GameState
{
	[Net] public bool EndEventEarly { get; set; }

	public EventState()
	{
		CurrentEvent = PlatesGame.EventManager.GetRandomEvent();
		NextStateTime = 30f;
	}

	public BaseEvent GetCurrentEvent()
	{
		return CurrentEvent; 
	}

	public override void OnEnter()
	{
		base.OnEnter();
		CurrentEvent?.OnEnter();
	}

	public override void OnExit()
	{
		base.OnExit();
		CurrentEvent?.OnExit();
	}

	public override void OnTick()
	{
		base.OnTick();
		CurrentEvent?.OnTick();

		if ( NextStateRealTime || EndEventEarly)
		{
			PlatesGame.ChangeState( new CooldownState
			{
				AllowPlayerJoins = true,
				HandleStateChanges = true,
				NextStateTime = 5f,
				NextState = new EventState()
			});
		}
	}
}
