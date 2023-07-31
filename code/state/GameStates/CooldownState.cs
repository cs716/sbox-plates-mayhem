using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Pl8Mayhem.state.GameStates
{
	public partial class CooldownState : GameState
	{
		public RealTimeUntil EndTime { get; set; }
		public GameState NextState { get; set; }

		public bool AllowPlayerJoin { get; set; } = false;
		
		public override void OnPlayerConnect( IClient client )
		{
			base.OnPlayerConnect( client );
			if (AllowPlayerJoin)
			{
				// Spawn them in 
			} else
			{
				// Send them to Spectator
			}
		}

		public override void OnTick()
		{
			base.OnTick();

			if ( EndTime )
			{
				Pl8Mayhem.ChangeState( NextState );
			}
		}

		public override void OnEnter()
		{
			base.OnEnter();
		}
	}
}
