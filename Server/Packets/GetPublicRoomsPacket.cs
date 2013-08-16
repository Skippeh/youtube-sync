using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.Packets
{
	public class GetPublicRoomsPacket : Packet
	{
		public GetPublicRoomsPacket() : base("getPublicRooms") {}

		public override void HandlePacket(Dictionary<string, object> data, IWebSocketConnection socket, RemoteConnectionInfo info, Room room, ref List<IWebSocketConnection> allSockets)
		{
			info.SendPublicRooms();
		}
	}
}