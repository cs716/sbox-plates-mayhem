using System.Linq;
using PlatesGame.Entity;
using PlatesGame.Entity.Player;
using Sandbox;

namespace PlatesGame.Event;

public abstract partial class BaseEvent : BaseNetworkable
{
	// Event Information
	public virtual string Name => "No Name";
	[Net] public string Description { get; set; } = "No Description";

	public virtual EventManager.EventType EventType => EventManager.EventType.UnclassifiedEvent;
	public virtual double EventWeight => 1d;
	public virtual bool IsSecret => false;
	
	// Affected players/plates/etc
	public virtual int MinAffected { get; set; } = 2;
	public virtual int MaxAffected { get; set; } = 4;

	public bool HasExited { get; private set; }

	public virtual void OnEnter() {}

	public virtual void OnExit()
	{
		foreach ( var player in Sandbox.Entity.All.OfType<PlatesPlayer>() )
			player.WasImpacted = false;
		
		HasExited = true;
	}

	public virtual void OnTick() { }
}
