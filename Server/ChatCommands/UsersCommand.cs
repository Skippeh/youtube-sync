using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.ChatCommands
{
	public class UsersCommand : ChatCommand
	{
		public UsersCommand() : base("users", 0) {}

		public override string NotEnoughArgumentsString
		{
			get { return ""; }
		}

		public override string HelpString
		{
			get { return "Gets a list of all users in your current room."; }
		}

		public override void HandleCommand(IWebSocketConnection socket, RemoteConnectionInfo info, Room room)
		{
			if (room != null)
			{
				var result = "Users in this room: ";

				foreach (var user in room.Sockets.OrderBy(socket2 => socket2.GetInfo().Name))
				{
					result += user.GetInfo().Name + ", ";
				}

				result = result.Remove(result.LastIndexOf(", "));

				info.SendChatMessage(result);
			}
			else
			{
				info.SendChatMessage("You need to join a room to use this command.", Helper.ServerErrorColor);
			}
		}
	}
}