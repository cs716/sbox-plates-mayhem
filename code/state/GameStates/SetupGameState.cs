using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Pl8Mayhem.entity;
using Sandbox;

namespace Pl8Mayhem.state.GameStates;

public class SetupGameState : GameState
{
	public override void OnEnter()
	{
		base.OnEnter();
		
		Pl8Mayhem.Instance.PlateManager.ClearBoard();

		Pl8Mayhem.Instance.PlateManager.CreateBoard();
		
		


	}
}
