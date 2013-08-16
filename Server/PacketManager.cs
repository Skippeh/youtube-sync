using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using Server.Packets;

namespace Server
{
	public static class PacketManager
	{
		private static readonly Dictionary<string, Packet> packets = new Dictionary<string, Packet>();

		private static SyncHandler syncHandler;

		public static void Initialize(SyncHandler handler)
		{
			syncHandler = handler;
			AddListener(new ConnectPacket());
			AddListener(new ChatPacket());
			AddListener(new SetVideoStatePacket());
			AddListener(new AddVideoPacket());
			AddListener(new GetPublicRoomsPacket());
			AddListener(new SetNameColorPacket());
		}

		public static void AddListener(Packet packet)
		{
			packets.Add(packet.Intent, packet);
		}

		public static bool HandlePacket(string intent, IWebSocketConnection socket, Dictionary<string, object> data)
		{
			if (packets.ContainsKey(intent))
			{
				// If any parameter in RequiredParameters doesn't exist in the incoming packet data, return false.
				if (packets[intent].RequiredParams.Any(keyParam => !data.ContainsKey(keyParam)))
				{
					return false;
				}

				var info = socket.GetInfo();
				packets[intent].HandlePacket(data, socket, info, info != null // room param
					                                                 ? info.GetRoom()
					                                                 : null, ref syncHandler.Sockets);
				return true;
			}

			Console.WriteLine("Unhandled packet: " + intent);
			return false;
		}
	}
}