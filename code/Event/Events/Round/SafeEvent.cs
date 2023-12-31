﻿namespace PlatesGame;

public class SafeEvent : BaseEvent
{
	public override EventManager.EventType EventType => EventManager.EventType.RoundEvent;
	public override double EventWeight => 0;
	public override string Name => "Safe Event";
	public override string Description => "A safe event where nothing happens";
}
