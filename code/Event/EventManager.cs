using System;
using System.Collections.Generic;
using System.Linq;
using PlatesGame.Event.Events;
using PlatesGame.Event.Events.Arena;
using PlatesGame.Event.Events.Player;
using PlatesGame.Event.Events.Players;
using Sandbox;

namespace PlatesGame.Event;

public class EventManager
{
	public enum EventType
	{
		ArenaEvent,
		PlateEvent,
		PlayerEvent,
		RoundEvent,
		UnclassifiedEvent
	}
	
	private readonly List<BaseEvent> Events = new();

	public EventManager()
	{
		AddEvent( new SafeEvent() );
		//AddEvent( new ArenaHighGravityEvent() );
		//AddEvent( new ArenaLowGravityEvent() );
		//AddEvent( new PlayerSwapEvent() );
		//AddEvent( new BarrelRainEvent() );
		//AddEvent( new LavaSpinnerEvent() );
		//AddEvent( new LandmineEvent() );
		AddEvent( new PlateShrinkEvent() );
	}

	private void AddEvent(BaseEvent newEvent) {
		Events.Add(newEvent);
	}

	public SafeEvent GetSafeEvent()
	{
		return Events.OfType<SafeEvent>().First();
	}

	public BaseEvent GetRandomEvent()
	{
		var totalWeight = Events.Sum( baseEvent => baseEvent.EventWeight );
		var randomValue = Random.Shared.Double( 0, totalWeight );
		
		foreach (var baseEvent in Events.Where(e => e.EventWeight > 0 ))
		{
			if ( randomValue <= baseEvent.EventWeight )
				return baseEvent;
			randomValue -= baseEvent.EventWeight;
		}

		return null;
	}
}
