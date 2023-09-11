using System;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public abstract partial class BaseEvent : BaseNetworkable
{
	public enum EventStates
	{
		Waiting,
		Delaying,
		Starting,
		Running,
		Stopping
	}

	public EventStates EventState { get; set; }

	// Event Information
	public virtual string Name => "No Name";
	public virtual string Description => "No Description";
	public virtual EventManager.EventType EventType => EventManager.EventType.UnclassifiedEvent;
	public virtual double EventWeight => 1d;
	public virtual float EventDuration => -1f;
	public virtual float EventBeginDelay => -1f;

	private RealTimeUntil _eventBeginDelay;
	public bool EventBegan = false; 
	
	// Affected players/plates/etc
	public virtual int MinAffected { get; set; } = 2;
	public virtual int MaxAffected { get; set; } = 4;

	public bool HasExited { get; private set; }

	public virtual void OnInvoked()
	{
		HasExited = false;
		EventBegan = false; 
		PlatesGame.EventDetails.EventId++;
		PlatesGame.EventDetails.EventName = Name;
		PlatesGame.EventDetails.EventDescription = Description;
		PlatesGame.EventDetails.AffectedEntities.Clear();

		if ( EventBeginDelay > 0f )
		{
			Log.Info( "Delaying for: " + EventBeginDelay );
			_eventBeginDelay = EventBeginDelay;
			EventState = EventStates.Delaying;
			return;
		}
		
		EventState = EventStates.Starting;
		OnPreEventStart();
	}

	public virtual void OnPreEventStart()
	{
		EventState = EventStates.Running;
		OnStart();
	}
	
	public virtual void OnStart()
	{
	}

	public virtual void OnExit()
	{
		PlatesGame.EventDetails.AffectedEntities.Clear();
		HasExited = true;
	}

	public void Tick()
	{
		if ( EventState is EventStates.Delaying && _eventBeginDelay)
		{
			EventState = EventStates.Starting;
			OnPreEventStart();
		}

		if ( EventState is EventStates.Running )
		{
			EventTick();
		}
	}

	// Will only run if EventState is "Running" 
	public virtual void EventTick()
	{
	}
}
