using System.Linq;
using PlatesGame.Entity;
using Sandbox;

namespace PlatesGame.State.GameStates;

public partial class CooldownState : GameState
{
	public override bool HandleStateChanges => true;
	
	public override void OnPlayerDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		if ( Game.IsClient )
			return;

		foreach (var plate in Sandbox.Entity.All.OfType<PlateEntity>().Where( p => p.PlateOwner == client ))
		{
			plate.IsDead = true;
		}
	}
}
