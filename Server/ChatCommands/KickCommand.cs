using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.ChatCommands
{
	public class KickCommand : ChatCommand
	{
		public KickCommand() : base("kick", 1) {}

		public override string NotEnoughArgumentsString
		{
			get { return "You need to specify a username."; }
		}

		public override string HelpString
		{
			get { return "Kicks the given user from the room."; }
		}

		public override void HandleCommand(IWebSocketConnection socket, RemoteConnectionInfo info, Room room)
		{
			if (room.IsOwner(socket))
			{
				var targetUser = room.GetUser(JoinArg());

				if (targetUser != null)
				{
					if (targetUser != socket)
					{
						room.KickUser(targetUser);
					}
					else
					{
						info.SendChatMessage("You can't kick yourself.", Helper.ServerErrorColor);
					}
				}
				else
				{
					info.SendChatMessage("There's no user with that name in the room.", Helper.ServerErrorColor);
				}
			}
			else
			{
				info.SendChatMessage("You need to be the owner to use this command.", Helper.ServerErrorColor);
			}
		}
	}
}