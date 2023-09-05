﻿using System;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public abstract partial class BaseEvent : BaseNetworkable
{
	// Event Information
	public virtual string Name => "No Name";
	[Net] public string Description { get; set; } = "No Description";

	public virtual EventManager.EventType EventType => EventManager.EventType.UnclassifiedEvent;
	public virtual double EventWeight => 1d;
	
	// Affected players/plates/etc
	public virtual int MinAffected { get; set; } = 2;
	public virtual int MaxAffected { get; set; } = 4;

	public bool HasExited { get; private set; }

	[Net] public string Seed { get; set; }

	public virtual void OnEnter()
	{
		Seed = Name + Random.Shared.NextSingle();
	}

	public virtual void OnExit()
	{
		foreach ( var player in Entity.All.OfType<PlatesPlayer>() )
			player.WasImpacted = false;

		foreach ( var plate in PlateManager.Plates() )
			plate.WasImpacted = false;
		
		HasExited = true;
	}

	public virtual void OnTick() { }
}
