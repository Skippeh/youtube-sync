using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	/// <summary>
	/// Youtube player state.
	/// </summary>
	public enum PlayState
	{
		Unstarted = -1,
		Ended = 0,
		Playing = 1,
		Paused = 2,
		Buffering = 3,
		Cued = 4,
	}
}