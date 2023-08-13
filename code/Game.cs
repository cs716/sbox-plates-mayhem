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
	
	[Net, Change( nameof( OnStateChange ) )] private GameState InternalGameState { get; set; }
	public static GameState State => Instance?.InternalGameState;

	private void OnStateChange( GameState oldState, GameState newState )
	{
		oldState?.OnExit();
		newState?.OnEnter();
	}

	[GameEvent.Tick]
	public static void OnTick()
	{
		if ( Game.IsClient && State is EventState state)
		{
			var currentEvent = state.GetCurrentEvent();
			DebugOverlay.ScreenText( $"Event: {currentEvent.Name} ({currentEvent.ShortName}) ({currentEvent.ClassName})", (int)DebugTextLocations.EventData );
			DebugOverlay.ScreenText( $"Desc: {currentEvent.Description}", (int)DebugTextLocations.EventData + 1 );
		}
		State?.OnTick();
	}

	public static void ChangeState( GameState newState )
	{
		Assert.NotNull( newState );

		Instance.InternalGameState?.OnExit();
		Instance.InternalGameState = newState;
		Instance.InternalGameState?.OnEnter();
	}
	
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		
		var pawn = new PlatesPlayer();
		client.Pawn = pawn;
		pawn.DressFromClient( client );
		
		State?.OnPlayerConnect( client );
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
