using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Linq;
using PlatesGame.Entity;
using PlatesGame.Entity.Player;
using PlatesGame.Event;
using PlatesGame.State;
using PlatesGame.State.GameStates;
using PlatesGame.util;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace PlatesGame;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
partial class PlatesGame : GameManager
{
	public enum DebugTextLocations
	{
		PlayerData = 0,
		StateData = 5,
		EventData = 10
	}
	private readonly EventManager Events = new();	
	public static PlatesGame Instance => Current as PlatesGame;
	public static EventManager EventManager => Instance.Events;
	
	public PlatesGame()
	{
		ChangeState( new WaitingState
		{
			AllowPlayerJoins = true,
			HandleStateChanges = false
		} );
	}
	
	[Net] private GameState InternalGameState { get; set; }
	[Net] private BaseEvent InternalGameEvent { get; set; }
	public static GameState CurrentState => Instance?.InternalGameState;
	public static BaseEvent CurrentEvent => Instance?.InternalGameEvent;

	// Debug remove this 
	private string _lastEvent = "None";
	
	[GameEvent.Tick]
	public static void OnTick()
	{
		if ( CurrentState is EventState)
		{
			if ( Instance._lastEvent != CurrentEvent?.Name )
			{
				Log.Info($"{(Game.IsServer ? "SERVER" : "CLIENT")} Event changed to: {CurrentEvent?.Name} - It was previously {Instance._lastEvent}"  );
				Instance._lastEvent = CurrentEvent?.Name;
				
				if ( !Game.IsClient )
					return;
				
			}
			DebugOverlay.ScreenText( $"Event: {CurrentEvent?.Name} ({CurrentEvent?.ClassName})", (int)DebugTextLocations.EventData );
			DebugOverlay.ScreenText( $"Desc: {CurrentEvent?.Description}", (int)DebugTextLocations.EventData + 1 );
		}
		CurrentState?.OnTick();
	}

	public static void ChangeState( GameState newState )
	{
		Assert.NotNull( newState );

		CurrentState?.OnExit();
		Instance.InternalGameState = newState;
		CurrentState?.OnEnter();
	}

	public static void ChangeEvent( BaseEvent newEvent )
	{
		Assert.NotNull( newEvent );

		if ( CurrentEvent?.HasExited == false)
			CurrentEvent?.OnExit();
		Instance.InternalGameEvent = newEvent;
		CurrentEvent?.OnEnter();
	}
	
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		
		var pawn = new PlatesPlayer();
		client.Pawn = pawn;
		pawn.DressFromClient( client );
		
		CurrentState?.OnPlayerConnect( client );
	}

	[ConCmd.Admin]
	public static void CreateBoard()
	{
		PlateManager.CreateBoard();
	}

	[ConCmd.Admin]
	public static void RespawnAll()
	{
		foreach (var platesPlayer in All.OfType<PlatesPlayer>())
		{
			platesPlayer.Respawn();
		}
	}
	
	/// <summary>
	/// Assigns a plate to each player and destroys the excess
	/// </summary>
	[ConCmd.Admin]
	public static void AssignPlates()
	{
		var playerCount = Game.Clients.Count;
		var curPlayer = 0;

		foreach(var plate in All.OfType<PlateEntity>().OrderBy(_ => Random.Shared.Double( 0, 100 )))
		{
			if(curPlayer >= playerCount)
			{
				plate.Delete();
			}
			else
			{
				var client = Game.Clients.ElementAt( curPlayer );
				plate.PlateOwner = client;
				plate.OwnerName = client.Name;
				if(client.Pawn is PlatesPlayer ply)
				{
					ply.Respawn();
					ply.OwnedPlate = plate;
					ply.Alive = true;
					ply.Position = plate.Position + Vector3.Up * 100.0f;
					ply.BaseVelocity = Vector3.Zero;
					ply.Velocity = Vector3.Zero;
				}
			}
			curPlayer++;
		}
	}
}
