using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public class LandmineEvent : BaseEvent
{
	public override double EventWeight => 1d;
	private RealTimeSince TimeSinceEnter;
	private bool MinesSpawned;

	public override string Name => "Landmines";
	
	public override int MinAffected => 1;
	public override int MaxAffected => 12;

	private ICollection<PlateEntity> ImpactedPlates { get; set; } = new List<PlateEntity>();

	public override void OnEnter()
	{
		base.OnEnter();

		if ( Game.IsClient )
			return;

		ImpactedPlates.Clear();
		TimeSinceEnter = 0;
		MinesSpawned = false;
		
		var livingPlates = PlateManager.Plates().Where( p => !p.IsDead ).OrderBy( x => Random.Shared.Double( 1, 100 ) ).ToList();
		
		var numPlatesImpacted = Random.Shared.Int( MinAffected, Math.Clamp( livingPlates.Count, MinAffected, MaxAffected ) );
		List<string> playerNames = new();
		
		for ( var i = 0; i < numPlatesImpacted; i++ )
		{
			ImpactedPlates.Add(livingPlates[i]);
			livingPlates[i].WasImpacted = true;
			playerNames.Add( livingPlates[i].OwnerName );
		}
		
		Description = $"Landmines will spawn on the plate{(numPlatesImpacted != 1 ? "s" : "")} owned by {StringFormatter.FormatPlayerNames( playerNames )}! Avoid setting them off!";
	}

	private void AssignPlates()
	{
		foreach (var plate in ImpactedPlates)
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
			state.EndEventEarly = true;
		}
	}

	public override void OnTick()
	{
		base.OnTick();

		if ( !Game.IsServer )
			return;

		if ( TimeSinceEnter <= 5f || MinesSpawned )
			return;

		AssignPlates();
		MinesSpawned = true; 

	}
}
