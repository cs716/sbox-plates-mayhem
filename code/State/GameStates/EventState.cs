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
		
		PlatesGame.ChangeEvent(PlatesGame.EventManager.GetRandomEvent());
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
				AllowPlayerJoins = true,
				HandleStateChanges = true,
				NextStateTime = 5f,
				NextState = new EventState()
			});
		}
	}
}
