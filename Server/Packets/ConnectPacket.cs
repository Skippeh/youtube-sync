using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.Packets
{
	public class ConnectPacket : Packet
	{
		public ConnectPacket() : base("connect", "name")
		{
			
		}

		public override void HandlePacket(Dictionary<string, object> data, IWebSocketConnection socket, RemoteConnectionInfo info, Room room, ref List<IWebSocketConnection> allSockets)
		{
			var name = data["name"] as string;
			if (name == "")
				name = socket.ConnectionInfo.ClientIpAddress;

			socket.SetInfo(new RemoteConnectionInfo(name, socket, Helper.GetMD5(name + DateTime.Now.ToLongTimeString())));

			Helper.SendQuick(socket, new Dictionary<string, object>
			                         {
				                         {"intent", "connectResult"},
				                         {"success", true},
				                         {"myName", name},
				                         {"id", socket.GetInfo().ID},
			                         });

			if (data.ContainsKey("room"))
			{
				var roomName = data["room"] as string;

				if (roomName != null)
				{
					if (Helper.VerifyName(roomName))
					{
						if (RoomManager.Exists(roomName))
						{
							Helper.VerifyFixUsername(RoomManager.Rooms[roomName], socket.GetInfo());
							RoomManager.AddUserToRoom(socket, roomName);
						}
						else
						{
							RoomManager.CreateRoom(roomName, socket);
						}
					}
				}
			}

			Console.WriteLine(name + "(" + Helper.IPString(socket) + ") connected");
		}
	}
}