using System;
using System.Linq;
using PlatesGame.Entity.Props;
using PlatesGame.State.GameStates;
using PlatesGame.util;
using Sandbox;

namespace PlatesGame.Event.Events.Players;

public class LandmineEvent : BaseEvent
{
	public override double EventWeight => 1d;
	private RealTimeSince TimeSinceEnter;
	private bool MinesSpawned;
	private int PlatesAffected;
	
	public override int MinAffected => 1;
	public override int MaxAffected => 12;
	public override void OnEnter()
	{
		base.OnEnter();

		TimeSinceEnter = 0;
		MinesSpawned = false;
		PlatesAffected = Random.Shared.Int(MinAffected, Math.Clamp(PlateManager.Plates().Count( p => !p.IsDead ), MinAffected, MaxAffected));
		
		Name = "Landmine Event";
		Description = $"Landmines will spawn on {PlatesAffected} plates! Avoid setting them off!";
		ShortName = "Landmines";
	}

	private void AssignPlates()
	{
		var plates = PlateManager.Plates().Where( p => !p.IsDead ).OrderBy( x => Random.Shared.Double( 1, 100 ) );
		var minesSpawned = 0;
		foreach (var plate in plates)
		{
			if ( minesSpawned >= PlatesAffected )
				continue;

			var plateScale = plate.GetSize();
			var mine = new LandmineEntity
			{
				Position = plate.Position + (Vector3.Up * 3f),
				Scale = plateScale,
				Parent = plate
			};
			mine.Position += Vector3.Left * Random.Shared.Int(-50,50) * plateScale;
			mine.Position += Vector3.Forward * Random.Shared.Int(-50,50) * plateScale;
			
			mine.Spawn();
			minesSpawned++;
		}
		if ( PlatesGame.State is EventState state )
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
