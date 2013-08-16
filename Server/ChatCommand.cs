using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server
{
	public abstract class ChatCommand
	{
		public string Command { get; private set; }

		public string[] Args { get; set; }

		public abstract string NotEnoughArgumentsString { get; }

		public abstract string HelpString { get; }

		public int MinimumArgCount { get; private set; }

		protected ChatCommand(string command, int minimumArgCount)
		{
			Command = command;
			MinimumArgCount = minimumArgCount;
		}

		public abstract void HandleCommand(IWebSocketConnection socket, RemoteConnectionInfo info, Room room);

		public string JoinArg()
		{
			return JoinArg(0);
		}

		/// <summary>Gets all args after startIndex summed into 1 string.</summary>
		public string JoinArg(int startIndex)
		{
			return JoinArg(startIndex, Args.Length - startIndex);
		}

		/// <summary>Gets all args after startIndex and every arg after "count" times summed into 1 string.</summary>
		public string JoinArg(int startIndex, int count)
		{
			if (count < 0)
				throw new ArgumentException("Count can't be below 0.", "count");

			if (startIndex >= Args.Length)
				throw new ArgumentException("Start index can't be higher than the amount of args.", "startIndex");

			if (startIndex + count > Args.Count())
				throw new ArgumentException("End index can't be higher than the amount of args.", "endIndex");

			return string.Join(" ", Args.Skip(startIndex).Take(count));
		}
	}
}