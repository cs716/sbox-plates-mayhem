using System;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public class ArenaLowGravityEvent : BaseEvent
{

	public override EventManager.EventType EventType => EventManager.EventType.ArenaEvent;
	public override double EventWeight => 1d;
	public override string Name => "Low Gravity";
	public override string Description => "The gravity in the arena will be lowered for everyone!";
	public override float EventBeginDelay => 8f;

	public override void EventBegin()
	{
		base.EventBegin();
		
		if ( Game.IsClient )
			return;
		
		PlatesGame.Instance.ArenaGravity -= Random.Shared.Int( 100, 250 );
		Log.Info("Gravity set to " + PlatesGame.Instance.ArenaGravity  );
		
		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEvent();
		}
	}
}
