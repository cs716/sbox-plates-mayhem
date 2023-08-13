﻿using System;
using System.Linq;
using PlatesGame.Entity.Player;
using PlatesGame.Entity.Props;
using Sandbox;

namespace PlatesGame.Event.Events.Arena;

public class BarrelRainEvent : BaseEvent
{
	public override EventManager.EventType EventType => EventManager.EventType.ArenaEvent;
	public override double EventWeight => 1d;

	public override void OnEnter()
	{
		base.OnEnter();
		
		Name = "Explosive Barrel Rain";
		ShortName = "Barrel Rain";
		Description = "Explosive barrels will fall from the heavens!";
	}

	public override void OnExit()
	{
		base.OnExit();

		if ( Game.IsClient )
			return;
		
		foreach (var prop in Sandbox.Entity.All.Where(p => p.Tags.Has("eventProp"  )  ))
		{
			prop.DeleteAsync(1f);
		}
	}

	public override void OnTick()
	{
		base.OnTick();

		if ( Game.IsClient )
			return;

		var rand = Random.Shared.Int( 1, 50 );
		var slamPlayerChance = Random.Shared.Int( 1, 4 );
		if ( rand != 1 )
			return;

		var barrel = new BarrelEntity
		{
			Position = new Vector3( Random.Shared.Int(-1500,1500), Random.Shared.Int(-1500,1500), 1000 ),
			Rotation = Rotation.From(new Angles(Random.Shared.Float()*360,Random.Shared.Float()*360,Random.Shared.Float()*360)),
			Velocity = new Vector3(0, 0, -100),
			Scale = 2f,
			Name = "Explosive Barrel",
			Health = 1f
		};
		if ( slamPlayerChance == 1 ) // Unlucky, Slam that shit on someones head
			barrel.Position = (Vector3)Sandbox.Entity.All.OfType<PlatesPlayer>().Where( p => p.Alive )
				.OrderBy( x => Random.Shared.Double( 1, 100 ) ).First()?.Position.WithZ( 5000 );
		barrel.SetModel("models/sbox_props/oil_drum/oil_drum_explosive.vmdl_c");
		barrel.Tags.Add("eventProp");
		barrel.Spawn();
		Log.Info( "Spawned barrel" );
	}
}