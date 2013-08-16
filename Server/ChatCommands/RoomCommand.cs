using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.ChatCommands
{
	public class RoomCommand : ChatCommand
	{
		public RoomCommand() : base("room", 1) {}

		public override string NotEnoughArgumentsString
		{
			get { return "You need to specify a new room name."; }
		}

		public override string HelpString
		{
			get { return "Transfer to the specified room."; }
		}

		public override void HandleCommand(IWebSocketConnection socket, RemoteConnectionInfo info, Room room)
		{
			var roomName = JoinArg(0);

			if (!Helper.VerifyName(roomName))
			{
				info.SendChatMessage("Invalid room name.", Helper.ServerErrorColor);
				return;
			}

			if (!RoomManager.Exists(roomName))
			{
				RoomManager.CreateRoom(roomName, socket);
			}
			else
			{
				if (room != RoomManager.Rooms[roomName])
					RoomManager.AddUserToRoom(socket, roomName);
				else
					info.SendChatMessage("You're already in this room.", Helper.ServerErrorColor);
			}

			Console.WriteLine(info.Name + "(" + Helper.IPString(socket) + ") changed room to " + roomName + ".");
		}
	}
}