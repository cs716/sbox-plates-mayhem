using System.Linq;
using Sandbox;

namespace PlatesGame;

public partial class CooldownState : GameState
{
	public override bool HandleStateChanges => true;
	
	public override void OnPlayerDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		if ( Game.IsClient )
			return;

		foreach (var plate in Entity.All.OfType<PlateEntity>().Where( p => p.PlateOwner == client ))
		{
			plate.IsDead = true;
		}
	}
}
