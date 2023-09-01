using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public partial class LavaSpinnerEvent : BaseEvent
{
	public override double EventWeight => 1d;
	public override int MinAffected => 1;

	private RealTimeUntil StartDelay;
	private bool PropsCreated;

	public override string Name => "Lava Wheel";
	
	private ICollection<PlateEntity> ImpactedPlates { get; set; } = new List<PlateEntity>();

	public override void OnEnter()
	{
		base.OnEnter();
		
		if ( Game.IsClient )
			return;
		
		StartDelay = 5f;
		PropsCreated = false;

		ImpactedPlates.Clear();
		
		var livingPlates = PlateManager.Plates().Where( p => !p.IsDead ).OrderBy( x => Random.Shared.Double( 1, 100 ) ).ToList();
		MaxAffected = livingPlates.Count;
		
		var numPlatesImpacted = Random.Shared.Int( MinAffected, Math.Clamp( livingPlates.Count, MinAffected, MaxAffected ) );
		List<string> playerNames = new();
		
		for ( var i = 0; i < numPlatesImpacted; i++ )
		{
			ImpactedPlates.Add(livingPlates[i]);
			livingPlates[i].WasImpacted = true;
			playerNames.Add( livingPlates[i].OwnerName );
		}
		
		Description = $"Lava spinners will spawn on the plate{(numPlatesImpacted != 1 ? "s" : "")} owned by {StringFormatter.FormatPlayerNames( playerNames )}! Jump to avoid them!";
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
		
		foreach (var plate in ImpactedPlates)
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

		PropsCreated = true;
		
		if ( PlatesGame.CurrentState is EventState state )
		{
			state.EndEventEarly = true;
		}
	}
}
