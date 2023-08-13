using System;
using System.Linq;
using PlatesGame.Entity.Props;
using PlatesGame.State.GameStates;
using PlatesGame.util;
using Sandbox;

namespace PlatesGame.Event.Events.Players;

public partial class LavaSpinnerEvent : BaseEvent
{
	public override double EventWeight => 1d;
	public override int MinAffected => 1;

	private RealTimeUntil StartDelay;
	private bool PropsCreated;

	public override void OnEnter()
	{
		StartDelay = 5f;
		PropsCreated = false;
		
		Name = "Lava Wheel";
		Description = "Lava will spawn on some plates! Jump to avoid it.";
		ShortName = "Lava Wheel";
		
		base.OnEnter();
	}

	public override void OnTick()
	{
		base.OnTick();

		if ( !Game.IsServer )
		{
			return;
		}

		if ( !StartDelay || PropsCreated )
			return;
		
		var plates = PlateManager.Plates().Where( p => p.IsDead == false )
			.OrderBy( x => Random.Shared.Double( 1, 100 ) ).ToList();

		MaxAffected = plates.Count;

		var platesAffected = Random.Shared.Int( MinAffected, MaxAffected );
		var platesUpdated = 0;
		foreach (var plate in plates)
		{
			if ( platesUpdated >= platesAffected )
				continue;
			var spinner = new LavaSpinnerEntity
			{
				Position = plate.Position + Vector3.Up * 5,
				Scale = plate.GetSize(),
				EnableTouch = true,
				EnableTouchPersists = true
			};
			
			spinner.Spawn();
			plate.AddEntity( spinner, true );
			platesUpdated++;
		}

		PropsCreated = true;
		
		if ( PlatesGame.State is EventState state )
		{
			state.EndEventEarly = true;
		}
	}
}
