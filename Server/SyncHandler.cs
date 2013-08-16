using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Server
{
	public class SyncHandler
	{
		public List<IWebSocketConnection> Sockets; // Not a property because it's passed as a reference to packets.

		public SyncHandler()
		{
			Sockets = new List<IWebSocketConnection>();
		}

		private void OnOpen(IWebSocketConnection socket)
		{
			//Console.WriteLine("OnOpen: " + Helper.IPString(socket));
			//Console.WriteLine("Connection count: " + Sockets.Count);
		}

		private void OnClose(IWebSocketConnection socket)
		{
			Sockets.Remove(socket);

			var info = socket.GetInfo();

			if (info != null)
			{
				Console.WriteLine(info.Name + "(" + Helper.IPString(socket) + ") disconnected.");
				RoomManager.ClearUser(socket);
			}

			//Console.WriteLine("OnClose: " + Helper.IPString(socket));
			//Console.WriteLine("Connection count: " + Sockets.Count);
		}

		private void OnMessage(string data, IWebSocketConnection socket)
		{
			//Console.WriteLine("Message: " + data);

			Dictionary<string, object> dict;

			try
			{
				dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
			}
			catch (Exception ex)
			{
				var info = socket.GetInfo();

				if (info != null)
					//socket.GetInfo().SendChatMessage("Invalid data received! (" + ex.Message +")", Helper.ServerExceptionColor);
					info.SendDisconnect("Invalid data received. (" + ex.Message + ")", Helper.ServerExceptionColor);

				socket.Close();
				return;
			}

			try
			{
				if (!PacketManager.HandlePacket((string)dict["intent"], socket, dict))
				{
					socket.GetInfo().SendChatMessage("Failed to handle packet: " + (string) dict["intent"]);
				}
			}
			catch (Exception ex)
			{
				var info = socket.GetInfo();
				if (info != null)
				{
					//info.SendChatMessage("Invalid request! (" + ex.Message + ")", Helper.ServerExceptionColor);
					info.SendDisconnect("Invalid request. (" + ex + ")", Helper.ServerExceptionColor);
					Console.WriteLine("(" + Helper.IPString(socket) + ")\n" + ex);

					Directory.CreateDirectory("Errors");
					var file = File.CreateText("Errors/" + DateTime.Now.ToString("yyyy_MM_dd-HH-mm-ss--fff") + ".txt");
					file.WriteLine(Helper.IPString(socket));
					file.WriteLine("\r\nException: " + ex);
					file.WriteLine("\r\nStacktrace: " + ex.StackTrace);
					file.WriteLine("\r\nMethod name: " + ex.TargetSite.Name);
					file.WriteLine("\r\nConnection info: " + JsonConvert.SerializeObject(socket.ConnectionInfo));
					file.WriteLine("\r\nData: " + data);
					file.Close();
				}

				socket.Close();
			}
		}

		private void OnBinary(byte[] bytes, IWebSocketConnection socket)
		{
			// Not used
		}

		public void AddSocket(IWebSocketConnection socket)
		{
			Sockets.Add(socket);
			socket.OnOpen += () => OnOpen(socket);
			socket.OnClose += () => OnClose(socket);
			socket.OnMessage += (data) => OnMessage(data, socket);
			socket.OnBinary += (data) => OnBinary(data, socket);
		}
	}
}