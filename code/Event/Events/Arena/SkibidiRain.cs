using System;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace PlatesGame;

public class SkibidiRain : BaseEvent
{
	public override EventManager.EventType EventType => EventManager.EventType.ArenaEvent;
	public override double EventWeight => 0.1d;
	
	public override float EventDuration => 30f;

	public override string Name => "Skibidi Toilets";

	private Model dropModel;
	private bool isFailover = true; 

	public SkibidiRain()
	{
		dropModel = Model.Load( "models/red_barrel.vmdl_c" );
		GetPackage();
	}

	private async void GetPackage()
	{
		var package = await Package.Fetch( "turd.skibidi_toilet", false );
		if ( package is not { PackageType: Package.Type.Model } || package.Revision == null )
		{
			Log.Error( "Failed to load Skibidi model - Reverting to red barrel" );
			return;
		}

		var model = package.GetMeta( "PrimaryAsset", "models/red_barrel.vmdl_c");
		await package.MountAsync();

		Precache.Add( model );
		dropModel = Model.Load( model );
		isFailover = false; 
	}

	public override void OnInvoked()
	{
		base.OnInvoked();

		if ( isFailover )
		{
			PlatesGame.EventDetails.EventName = "Explosive Barrel Rain";
			PlatesGame.EventDetails.EventDescription = "Explosive barrels will fall from the heavens! Try to avoid them!";
			return; 
		}
		PlatesGame.EventDetails.EventName = "Skibidi Invasion";
		PlatesGame.EventDetails.EventDescription = "Explosive barr.. wait.. Those aren't barrels!";
	}
	
	public override void OnExit()
	{
		base.OnExit();

		if ( Game.IsClient )
			return;
		
		foreach (var prop in Entity.All.Where(p => p.Tags.Has("eventProp"  )  ))
		{
			prop.DeleteAsync(1f);
		}
	}

	public override void EventTick()
	{
		base.EventTick();

		if ( Game.IsClient )
			return;

		var rand = Random.Shared.Int( 1, 50 );
		var slamPlayerChance = Random.Shared.Int( 1, 10 );
		if ( rand != 1 )
			return;

		var barrel = new FallingProp
		{
			Position = new Vector3( Random.Shared.Int(-1500,1500), Random.Shared.Int(-1500,1500), 5000 ),
			Rotation = Rotation.From(new Angles(Random.Shared.Float()*360,Random.Shared.Float()*360,Random.Shared.Float()*360)),
			Velocity = new Vector3(0, 0, 0),
			Scale = Random.Shared.Float( 0.8f, 1.5f ),
			Name = "Skibidi Toilet",
			BlastRadius = 120f,
			Health = 1f
		};
		if ( slamPlayerChance == 1 ) // Unlucky, Slam that shit on someones head
			barrel.Position = (Vector3)Players.GetLiving()
				.OrderBy( x => Random.Shared.Double( 1, 100 ) ).First().Position.WithZ( 5000 );
		barrel.Model = dropModel;
		barrel.Tags.Add("eventProp");
		barrel.Spawn();

		if ( !isFailover )
		{
			barrel.PlaySound( "skibidi" );
		}
	}
}
