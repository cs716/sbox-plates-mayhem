﻿using System.Linq;
using PlatesGame.Entity.Player;
using Sandbox;

namespace PlatesGame.Event.Events.Arena;

public class ArenaHighGravityEvent : BaseEvent
{
	public override EventManager.EventType EventType => EventManager.EventType.ArenaEvent;
	public override double EventWeight => 1d;
	public override string Name => "High Gravity";

	public override void OnEnter()
	{
		base.OnEnter();
		
		Description = "The gravity in the arena will be raised for everyone!";
		

		if ( Game.IsClient )
			return;
		
		foreach (var player in Sandbox.Entity.All.OfType<PlatesPlayer>().Where( p => p.Alive  ))
		{
			player.Controller.Gravity += GameConfig.DefaultGravity * 0.5f;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		
		if ( Game.IsClient )
			return;
		
		foreach (var player in Sandbox.Entity.All.OfType<PlatesPlayer>().Where(p => p.Controller?.Gravity.AlmostEqual( GameConfig.DefaultGravity ) == false ))
		{
			player.Controller.Gravity = GameConfig.DefaultGravity;
		}
	}
}
