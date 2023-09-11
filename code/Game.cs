using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Linq;
using PlatesGame.UI;

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
	private readonly EventManager Events = new();	
	public static PlatesGame Instance => Current as PlatesGame;
	public static EventManager EventManager => Instance.Events;

	private PlatesHud _hud;
	
	public PlatesGame()
	{
		ChangeState( new WaitingState() );
		if ( Game.IsClient )
		{
			_hud = new PlatesHud();
		}
		else
		{
			ArenaGravity = GameConfig.DefaultGravity;
		}
	}
	
	[Net] private GameState InternalGameState { get; set; }
	[Net] private BaseEvent InternalGameEvent { get; set; }

	[Net] private CurrentEventDetails InternalEventDetails { get; set; } = new();

	public static GameState CurrentState => Instance?.InternalGameState;
	public static BaseEvent CurrentEvent => Instance?.InternalGameEvent;

	public static CurrentEventDetails EventDetails => Instance?.InternalEventDetails; 
	
	public static PlatesHud PlayerHud => Instance?._hud;

	[Net] public float ArenaGravity { get; set; }

	private int _eventId = -1; 
	
	[GameEvent.Tick]
	public void OnTick()
	{
		if ( CurrentState is EventState)
		{
			if ( EventDetails != null && EventDetails.EventId != _eventId )
			{
				Log.Info($"{(Game.IsServer ? "SERVER" : "CLIENT")} Event changed to: {EventDetails?.EventName}"  );

				if ( Game.IsClient )
					PlayerHud.AddChild( new LargeNotification(7f) );

				_eventId = EventDetails!.EventId;
			}
		}
		CurrentState?.OnTick();
	}

	[ConCmd.Client]
	public static void TestSplat()
	{
		_ = new HitSplat(Game.LocalPawn, HitSplat.DmgType.Regular, 10);
	}

	[ConCmd.Client]
	public static void TestNotification()
	{
		PlayerHud.AddChild( new LargeNotification( "Test Notification", "Test Subtitle", 7f ) );
	}

	public static void ChangeState( GameState newState )
	{
		Assert.NotNull( newState );
		Log.Info("Called ChangeState - Set state to " + newState.ClassName  );

		CurrentState?.OnExit();
		Instance.InternalGameState = newState;
		CurrentState?.OnEnter();
	}

	public static void ChangeEvent( BaseEvent newEvent )
	{
		if ( newEvent == null )
			return;

		if ( CurrentEvent?.HasExited == false)
			CurrentEvent?.OnExit();
		
		newEvent.OnInvoked();
		Instance.InternalGameEvent = newEvent;
	}
	
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		
		var pawn = new PlatesPlayer();
		client.Pawn = pawn;
		pawn.DressFromClient( client );
		if ( client.IsBot )
			pawn.Tags.Add( "input_disabled" );
		
		CurrentState?.OnPlayerConnect( client );
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		CurrentState?.OnPlayerDisconnect( cl, reason );
	}
}
