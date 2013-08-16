using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.ChatCommands
{
	public class HelpCommand : ChatCommand
	{
		public HelpCommand() : base("help", 0) {}

		public override string NotEnoughArgumentsString
		{
			get { return ""; }
		}

		public override string HelpString
		{
			get { return "Returns all commands, or help for the specified command."; }
		}

		public override void HandleCommand(IWebSocketConnection socket, RemoteConnectionInfo info, Room room)
		{
			if (Args.Length > 0)
			{
				var command = CommandManager.GetCommand(Args[0].ToLower());

				if (command != null)
				{
					var capitalized = command.Command.Capitalize();
					socket.GetInfo().SendChatMessage(capitalized + ": " + command.HelpString);
					socket.GetInfo().SendChatMessage("Minimum required arguments: " + command.MinimumArgCount);
				}
				else
				{
					socket.GetInfo().SendChatMessage("There is no command with this name.", Helper.ServerErrorColor);
				}
			}
			else
			{
				string result = "-- Commands --\n";

				foreach (var command in CommandManager.GetCommands())
				{
					result += "• " + command.Command.Capitalize() + ": " + command.HelpString + "\n";
				}

				result += "-- End of commands --";

				info.SendChatMessage(result);
			}
		}
	}
}