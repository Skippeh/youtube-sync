using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
				try
				{
					server.Start(ServerConfig);
				}
				catch (SocketException ex)
				{
					if (ex.ErrorCode == 10048)
					{
						Console.WriteLine("Something is already running on this port. Can't start server.");
						Environment.Exit(1);
					}
				}

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