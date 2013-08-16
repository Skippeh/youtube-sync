using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.ChatCommands
{
	public class NameCommand : ChatCommand
	{
		public NameCommand() : base("name", 1)
		{
			
		}

		public override string NotEnoughArgumentsString
		{
			get { return "You need to specify a new name."; }
		}

		public override string HelpString
		{
			get { return "Rename yourself."; }
		}

		public override void HandleCommand(IWebSocketConnection socket, RemoteConnectionInfo info, Room room)
		{
			var oldName = info.Name;
			string newName = Helper.GetUserName(JoinArg(), info.GetRoom());

			if (!Helper.VerifyName(newName))
			{
				info.SendChatMessage("Invalid name.", Helper.ServerErrorColor);
				return;
			}

			if (oldName == newName)
			{
				info.SendChatMessage("You already have that name.", Helper.ServerErrorColor);
				return;
			}

			info.Name = newName;

			info.SendSetName(newName, room == null, newName == JoinArg()); // permanent = if true, the client will know this name is permanent, and not a rename. different from what's expected. (example: rename to Name, becomes Name(1))

			if (room != null)
				room.SendChatMessageToAll(oldName + " changed name to " + newName + ".");
			else
				info.SendChatMessage("Name set to " + newName + ".");
		}
	}
}