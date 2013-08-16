using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fleck;

namespace Server
{
	class Program
	{
		private static WebSocketServer server;
		private static SyncHandler syncHandler;

		static void Main(string[] args)
		{
			Globals.Initialize();
			YoutubeHelper.Initialize();
			CommandManager.Initialize();
			syncHandler = new SyncHandler();
			PacketManager.Initialize(syncHandler);

			using (server = new WebSocketServer(Globals.MyLocation))
			{
				server.Start(ServerConfig);

				while (true)
				{
					Thread.Sleep(100);
				}
			}
		}

		private static void ServerConfig(IWebSocketConnection socket)
		{
			syncHandler.AddSocket(socket);
		}
	}
}