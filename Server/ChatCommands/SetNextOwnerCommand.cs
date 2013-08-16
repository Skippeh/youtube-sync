using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.ChatCommands
{
	public class SetNextOwnerCommand : ChatCommand
	{
		public SetNextOwnerCommand() : base("setnextowner", 1) {}

		public override string NotEnoughArgumentsString
		{
			get { return "you need to specify a username."; }
		}

		public override string HelpString
		{
			get { return "Sets the next owner of this room when the current owner leaves. Only usable by owners."; }
		}

		public override void HandleCommand(IWebSocketConnection socket, RemoteConnectionInfo info, Room room)
		{
			if (room != null)
			{
				if (room.Owner == socket)
				{
					var nextOwner = room.GetUser(JoinArg());

					if (nextOwner != null)
					{
						if (nextOwner != socket)
						{
							room.NextOwner = nextOwner;
							info.SendChatMessage("Next owner is now " + nextOwner.GetInfo().Name + ".");
							nextOwner.GetInfo().SendChatMessage("You are now the next owner of this room.", Helper.ServerRoomColor);
						}
						else
						{
							info.SendChatMessage("You are already the owner of this room.", Helper.ServerErrorColor);
						}
					}
					else
					{
						info.SendChatMessage("There's no user in this room with that name.", Helper.ServerErrorColor);
					}
				}
				else
				{
					info.SendChatMessage("You need to be the owner of the room to use this command.", Helper.ServerErrorColor);
				}
			}
			else
			{
				info.SendChatMessage("You need to join a room to use this command.", Helper.ServerErrorColor);
			}
		}
	}
}