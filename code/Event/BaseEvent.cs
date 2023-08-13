using System.Linq;
using PlatesGame.Entity;
using PlatesGame.Entity.Player;
using Sandbox;

namespace PlatesGame.Event;

public abstract partial class BaseEvent : BaseNetworkable
{
	// Event Information
	[Net] public string Name { get; set; } = "No Name";
	[Net] public string Description { get; set; } = "No Description";
	[Net] public string ShortName { get; set; } = "None";

	public virtual EventManager.EventType EventType => EventManager.EventType.UnclassifiedEvent;
	public virtual double EventWeight => 1d;
	public virtual bool IsSecret => false;
	
	// Affected players/plates/etc
	public virtual int MinAffected { get; set; } = 2;
	public virtual int MaxAffected { get; set; } = 4;
	
	// Enter/Exit functions called when the event is invoked
	public virtual void OnEnter( PlatesPlayer playerOverride )
	{
		OnEnter();
	}

	public virtual void OnEnter( PlateEntity plateOverride )
	{
		OnEnter();
	}

	public virtual void OnEnter() {
		
	}

	public virtual void OnExit()
	{
		foreach ( var player in Sandbox.Entity.All.OfType<PlatesPlayer>() )
			player.WasImpacted = false;
	}

	public virtual void OnTick()
	{
		if ( Game.IsClient )
		{
			DebugOverlay.ScreenText( $"Event: {Name} ({ShortName}) ({ClassName})", (int)PlatesGame.DebugTextLocations.EventData );
			DebugOverlay.ScreenText( $"Desc: {Description}", (int)PlatesGame.DebugTextLocations.EventData + 1 );
		}
	}
}
