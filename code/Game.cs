using Sandbox;
using Sandbox.Diagnostics;
using Pl8Mayhem.state;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pl8Mayhem.entity;
using Pl8Mayhem.state.GameStates;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace Pl8Mayhem;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
partial class Pl8Mayhem : GameManager
{
	public static Pl8Mayhem Instance => Current as Pl8Mayhem;
	public PlateManager PlateManager = new();

	public Pl8Mayhem()
	{
		ChangeState( new WaitingState() );
	}
	
	[Net, Change( nameof( OnStateChange ) )] private GameState InternalGameState { get; set; }
	public static GameState State => Instance?.InternalGameState;

	private void OnStateChange( GameState oldState, GameState newState )
	{
		oldState?.OnExit();
		newState?.OnEnter();
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
		
		State?.OnPlayerConnect( client );
	}

	[ConCmd.Admin]
	public static void CreateBoard()
	{
		Instance.PlateManager.CreateBoard();
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

		foreach(var plate in All.OfType<PlateEntity>().OrderBy(x => Random.Shared.Double( 0, 100 )))
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
