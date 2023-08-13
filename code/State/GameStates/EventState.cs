using PlatesGame.Event;
using Sandbox;

namespace PlatesGame.State.GameStates;

public partial class EventState : GameState
{
	public override float NextStateTime { get; set; } = 30f;

	[Net] private BaseEvent CurrentEvent { get; set; }
	public bool EndEventEarly = false;

	public EventState()
	{
		if ( Game.IsServer )
		{
			CurrentEvent = PlatesGame.EventManager.GetRandomEvent();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		
		
		
		if ( Game.IsClient )
			return;
		CurrentEvent?.OnEnter();
	}

	public override void OnExit()
	{
		base.OnExit();
		
		
		if ( Game.IsClient )
			return;
		CurrentEvent?.OnExit();
	}

	public override void OnTick()
	{
		base.OnTick();
		CurrentEvent?.OnTick();
		
		
		if ( Game.IsClient )
			return;

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
