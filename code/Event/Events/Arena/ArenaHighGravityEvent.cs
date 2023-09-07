using System;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public class ArenaHighGravityEvent : BaseEvent
{
	public override EventManager.EventType EventType => EventManager.EventType.ArenaEvent;
	public override double EventWeight => 1d;
	public override string Name => "High Gravity";
	public override string Description => "The gravity in the arena will be raised for everyone!";

	public override float EventDuration => 20f;
	public override float EventBeginDelay => 8f;
	public override void EventBegin()
	{
		base.EventBegin();
		
		if ( Game.IsClient )
			return;
		
		foreach (var player in Entity.All.OfType<PlatesPlayer>().Where( p => p.LifeState is LifeState.Alive  ))
		{
			player.Controller.Gravity += Random.Shared.Int( 50, 150 );
		}
		
		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEvent();
		}
	}
}
