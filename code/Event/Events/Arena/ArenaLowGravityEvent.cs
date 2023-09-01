using System.Linq;
using Sandbox;

namespace PlatesGame;

public class ArenaLowGravityEvent : BaseEvent
{

	public override EventManager.EventType EventType => EventManager.EventType.ArenaEvent;
	public override double EventWeight => 1d;
	public override string Name => "Low Gravity";
	
	public override void OnEnter()
	{
		base.OnEnter();
		
		Description = "The gravity in the arena will be lowered for everyone!";

		if ( Game.IsClient )
			return;
		
		foreach (var player in Entity.All.OfType<PlatesPlayer>().Where( p => p.LifeState is LifeState.Alive  ))
		{
			player.Controller.Gravity -= GameConfig.DefaultGravity * 0.5f;
		}
	}
	
	public override void OnExit()
	{
		base.OnExit();
		

		if ( Game.IsClient )
			return;
		
		foreach (var player in Entity.All.OfType<PlatesPlayer>().Where(p => p.Controller?.Gravity.AlmostEqual( GameConfig.DefaultGravity ) == false ))
		{
			player.Controller.Gravity = GameConfig.DefaultGravity;
		}
	}
}
