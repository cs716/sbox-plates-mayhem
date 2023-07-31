using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Pl8Mayhem.state
{
	public abstract partial class GameState : BaseNetworkable
	{
		public virtual void OnEnter() { }
		public virtual void OnExit() { }
		public virtual void OnPlayerConnect( IClient client ) { }

		public virtual void OnTick() { }
	}
}
