using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.Packets
{
	public class SetNameColorPacket : Packet
	{
		public SetNameColorPacket() : base("setNameColor", "color") {}

		public override void HandlePacket(Dictionary<string, object> data, IWebSocketConnection socket, RemoteConnectionInfo info, Room room, ref List<IWebSocketConnection> allSockets)
		{
			var color = (string) data["color"];

			if (Helper.VerifyNameColor(color))
			{
				info.NameColor = color;
			}
			else
			{
				info.SendChatMessage("\"" + color + "\" is not an accepted color.", Helper.ServerErrorColor);
			}
		}
	}
}