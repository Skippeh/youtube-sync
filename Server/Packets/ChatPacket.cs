using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.Packets
{
	public class ChatPacket : Packet
	{
		public ChatPacket() : base("chat", "message") {}

		public override void HandlePacket(Dictionary<string, object> data, IWebSocketConnection socket, RemoteConnectionInfo info, Room room, ref List<IWebSocketConnection> allSockets)
		{
			var message = (string) data["message"];

			if (message.StartsWith("/"))
			{
				if (!CommandManager.HandleCommand(message, socket))
				{
					info.SendChatMessage("Unknown command.", Helper.ServerErrorColor);
				}
			}
			else
			{
				if (room != null)
				{
					room.SendChatMessageToAll(message, info.Name, "#FFF", info.NameColor);
				}
				else
				{
					info.SendChatMessage("You need to join a room to use the chat. Type /room [room name] to join the room called [room name].", Helper.ServerErrorColor);
				}
			}
		}
	}
}