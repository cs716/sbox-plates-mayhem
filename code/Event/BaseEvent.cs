using System;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public abstract partial class BaseEvent : BaseNetworkable
{
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

	public virtual void OnEnter()
	{
		EventBegan = false; 
		PlatesGame.EventDetails.EventId++;
		PlatesGame.EventDetails.EventName = Name;
		PlatesGame.EventDetails.EventDescription = Description;
		PlatesGame.EventDetails.AffectedEntities.Clear();

		if ( EventBeginDelay > 0 )
		{
			_eventBeginDelay = EventBeginDelay; 
		}
	}

	public virtual void EventBegin()
	{
		EventBegan = true; 
	}

	public virtual void OnExit()
	{
		PlatesGame.EventDetails.AffectedEntities.Clear();
		HasExited = true;
	}

	public virtual void OnTick()
	{
		if ( EventBeginDelay > 0 && _eventBeginDelay && !EventBegan )
			EventBegin();
	}
}
