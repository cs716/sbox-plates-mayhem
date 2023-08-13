using System;
using System.Collections.Generic;
using System.Linq;
using PlatesGame.Entity;
using PlatesGame.Entity.Player;
using Sandbox;
// ReSharper disable CheckNamespace

namespace PlatesGame.util;

public static class PlateManager
{
	public static IEnumerable<PlateEntity> Plates()
	{
		return Sandbox.Entity.All.OfType<PlateEntity>();
	}

	public static void CreateBoard()
	{
		for(var i=-4; i<4; i++){
			for(var j=-4; j<4; j++)
			{
				// ReSharper disable once ObjectCreationAsStatement
				new PlateEntity(new Vector3((i+0.5f)*92*4,(j+0.5f)*92*4,0), 1, "Nobody")
				{
					//EnableDrawing = false
				};
			}
		}
	}

	public static void ClearBoard()
	{
		foreach (var plateEntity in Plates())
		{
			plateEntity.Delete();
		}
	}

	private static void AssignPlate( PlatesPlayer player, PlateEntity plate )
	{
		if ( Game.IsClient )
			return; 
		
		plate.PlateOwner = player.Client;
		plate.OwnerName = player.Name;
		plate.EnableDrawing = true;
		plate.IsDead = false;
		player.OwnedPlate = plate;
		player.Alive = true;
		player.Position = plate.Position + Vector3.Up * 100.0f;
		player.BaseVelocity = Vector3.Zero;
		player.Velocity = Vector3.Zero;
		player.Respawn();
	}

	public static void FindAndAssignPlate( PlatesPlayer player )
	{
		AssignPlate( player, FindEmptyPlate() );
	}

	public static void FindAndAssignPlate( IClient client )
	{
		if (client.Pawn is PlatesPlayer player)
		{
			FindAndAssignPlate( player );
		}
	}

	private static PlateEntity FindEmptyPlate()
	{
		return Sandbox.Entity.All.OfType<PlateEntity>()
			.Where( p => p.PlateOwner == null )
			//.OrderBy( _ => Random.Shared.Double( 0, 100 ) )
			.First();
	}

	public static void CreatePlates( IEnumerable<IClient> players)
	{
		foreach (var client in players)
		{
			if ( client.Pawn is not PlatesPlayer player )
			{
				continue;
			}

			var plate = FindEmptyPlate();

			AssignPlate( player, plate );
		}
	}

	public static void CleanUnusedPlates()
	{
		foreach (var plate in Sandbox.Entity.All.OfType<PlateEntity>().Where(p => p.PlateOwner == null  ))
		{
			plate.Delete();
		}
	}
}
