using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.ChatCommands
{
	class TogglePrivilegedUserCommand : ChatCommand
	{
		public TogglePrivilegedUserCommand() : base("toggleprivuser", 1) {}

		public override string NotEnoughArgumentsString
		{
			get { return "You need to specify a user."; }
		}

		public override string HelpString
		{
			get { return "Toggles privileges on the specified user."; }
		}

		public override void HandleCommand(IWebSocketConnection socket, RemoteConnectionInfo info, Room room)
		{
			var username = JoinArg();

			if (room != null)
			{
				if (room.IsOwner(socket))
				{
					var targetUser = room.GetUser(username);

					if (targetUser != null)
					{
						if (targetUser != socket)
						{
							if (!room.IsPrivileged(targetUser))
							{
								targetUser.GetInfo().SendAndSetPrivileged(true);
								info.SendChatMessage(targetUser.GetInfo().Name + " is now a privileged user.");
							}
							else
							{
								targetUser.GetInfo().SendAndSetPrivileged(false);
								info.SendChatMessage(targetUser.GetInfo().Name + " is no longer a privileged user.");
							}
						}
						else
						{
							info.SendChatMessage("You are already privileged by being the owner.", Helper.ServerErrorColor);
						}
					}
					else
					{
						info.SendChatMessage("There's no user with that name in this room.", Helper.ServerErrorColor);
					}
				}
				else
				{
					info.SendChatMessage("You need to be the owner of the room to use this command.", Helper.ServerErrorColor);
				}
			}
			else
			{
				info.SendChatMessage("You need to join a room before using this command.", Helper.ServerErrorColor);
			}
		}
	}
}