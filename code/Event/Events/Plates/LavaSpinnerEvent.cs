using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public partial class LavaSpinnerEvent : BaseEvent
{
	public override double EventWeight => 1d;
	public override int MinAffected => 1;
	public override string Name => "Lava Wheel";

	public override float EventBeginDelay => 10f;

	public override void OnEnter()
	{
		base.OnEnter();
		
		if ( Game.IsClient )
			return;
		
		var livingPlates = PlateManager.Plates().Where( p => !p.IsDead ).OrderBy( x => Random.Shared.Double( 1, 100 ) ).ToList();
		MaxAffected = livingPlates.Count;
		
		var numPlatesImpacted = Random.Shared.Int( MinAffected, Math.Clamp( livingPlates.Count, MinAffected, MaxAffected ) );
		List<string> playerNames = new();
		
		for ( var i = 0; i < numPlatesImpacted; i++ )
		{
			PlatesGame.EventDetails.AffectedEntities.Add( livingPlates[i] );
			playerNames.Add( livingPlates[i].OwnerName );
		}
		
		PlatesGame.EventDetails.EventDescription = $"Lava spinners will spawn on the plate{(numPlatesImpacted != 1 ? "s" : "")} owned by {StringFormatter.FormatPlayerNames( playerNames )}! Jump to avoid them!";
	}

	public override void EventBegin()
	{
		base.EventBegin();

		if ( Game.IsClient )
			return;
		
		foreach (var plate in PlatesGame.EventDetails.AffectedEntities.OfType<PlateEntity>().Where(p => !p.IsDead  ))
		{
			var spinner = new LavaSpinnerEntity
			{
				Position = plate.Position + Vector3.Up * 5,
				Scale = plate.GetSize(),
				EnableTouch = true,
				EnableTouchPersists = true
			};
			
			spinner.Spawn();
			plate.AddEntity( spinner, true );
		}
		
		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEvent();
		}
	}
}
