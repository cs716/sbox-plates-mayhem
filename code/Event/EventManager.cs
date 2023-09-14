using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace PlatesGame;

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
		// Safe
		AddEvent( new SafeEvent() );
		
		AddEvent( new ArenaHighGravityEvent() );
		AddEvent( new ArenaLowGravityEvent() );
		AddEvent( new BarrelRainEvent() );
		AddEvent( new SkibidiRain() );
		AddEvent( new GlassPlates() );
		//AddEvent( new PoisonPlates() );

		AddEvent( new LavaSpinnerEvent() );
		AddEvent( new LandmineEvent() );
		AddEvent( new PlateGrowEvent() );
		AddEvent( new PlateShrinkEvent() );

		AddEvent( new PlayerSwapEvent() );
		AddEvent( new PlayerInfectionEvent() );
	}

	private void AddEvent(BaseEvent newEvent) {
		Events.Add(newEvent);
	}

	[ConCmd.Admin]
	public static void SetNextEvent(string eventName)
	{
		PlatesGame.EventManager.DebugNextEvent = eventName; 
	}

	public string DebugNextEvent; 

	public BaseEvent GetRandomEvent()
	{
		var totalWeight = Events.Sum( baseEvent => baseEvent.EventWeight );
		var randomValue = Random.Shared.Double( 0, totalWeight );
		var currentEvent = PlatesGame.CurrentEvent != null ? PlatesGame.CurrentEvent.ClassName : "None";
		Log.Info("Current Exiting Event: " + currentEvent  );

		if ( !string.IsNullOrEmpty( DebugNextEvent ) )
		{
			var nextEvent = Events.Where( e => string.Equals(e.ClassName, DebugNextEvent, StringComparison.CurrentCultureIgnoreCase) );
			var eventsList = nextEvent.ToList();
			DebugNextEvent = string.Empty;
			if ( eventsList.Count == 1 )
				return eventsList.First();
		}
		
		foreach (var baseEvent in Events.Where( e => e.ClassName != currentEvent).Where(e => e.EventWeight > 0 ))
		{
			if ( randomValue <= baseEvent.EventWeight )
				return baseEvent;
			randomValue -= baseEvent.EventWeight;
		}

		return null;
	}
}

public partial class CurrentEventDetails : BaseNetworkable
{
	public float LastUpdate;
	[Net, Change(nameof(PropertyChanged))] public string EventName { get; set; } = "None";
	[Net, Change(nameof(PropertyChanged))] public string EventDescription { get; set; } = "None";
	[Net, Change(nameof(PropertyChanged))] public IList<Entity> AffectedEntities { get; set; }
	[Net, Change(nameof(PropertyChanged))] public int EventId { get; set; }

	public void PropertyChanged()
	{
		LastUpdate = RealTime.Now;
	}
}
