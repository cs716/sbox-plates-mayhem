﻿using System;
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
	public override void OnStart()
	{
		base.OnStart();
		
		if ( Game.IsClient )
			return;
		
		PlatesGame.Instance.ArenaGravity += Random.Shared.Int( 100, 250 );
		Log.Info("Gravity set to " + PlatesGame.Instance.ArenaGravity  );
		
		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEvent();
		}
	}
}
