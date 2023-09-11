using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public class LandmineEvent : BaseEvent
{
	public override double EventWeight => 1d;

	public override string Name => "Landmines";
	
	public override int MinAffected => 1;
	public override int MaxAffected => 12;

	public override float EventBeginDelay => 10f;

	public override void OnInvoked()
	{
		base.OnInvoked();

		if ( Game.IsClient )
			return;
		
		var livingPlates = PlateManager.Plates().Where( p => !p.IsDead ).OrderBy( x => Random.Shared.Double( 1, 100 ) ).ToList();
		
		var numPlatesImpacted = Random.Shared.Int( MinAffected, Math.Clamp( livingPlates.Count, MinAffected, MaxAffected ) );
		List<string> playerNames = new();
		
		for ( var i = 0; i < numPlatesImpacted; i++ )
		{
			PlatesGame.EventDetails.AffectedEntities.Add( livingPlates[i] );
			playerNames.Add( livingPlates[i].OwnerName );
		}
		
		PlatesGame.EventDetails.EventDescription = $"Landmines will spawn on the plate{(numPlatesImpacted != 1 ? "s" : "")} owned by {StringFormatter.FormatPlayerNames( playerNames )}! Avoid setting them off!";
	}

	public override void OnStart()
	{
		base.OnStart();

		if ( Game.IsClient )
			return;
		
		foreach (var plate in PlatesGame.EventDetails.AffectedEntities.OfType<PlateEntity>().Where(p => !p.IsDead  ))
		{
			var plateScale = plate.GetSize();
			var mine = new LandmineEntity
			{
				Position = plate.Position + (Vector3.Up * 3f),
				Parent = plate
			};
			mine.Position += Vector3.Left * Random.Shared.Int(-50,50) * plateScale;
			mine.Position += Vector3.Forward * Random.Shared.Int(-50,50) * plateScale;
			
			mine.Spawn();
		}
		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEvent();
		}
	}
}
