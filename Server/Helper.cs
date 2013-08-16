using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using Newtonsoft.Json;

namespace Server
{
	public static class Helper
	{
		public const string ServerMessageColor = "rgb(189, 213, 255)";
		public const string ServerErrorColor = "rgb(228, 94, 94)";
		public const string ServerExceptionColor = "rgb(224, 115, 14)";
		public const string ServerRoomColor = "rgb(123, 209, 44)";

		public const string DefaultNameColor = "#FFF";

		public static string CreateQuick(Dictionary<string, object> args)
		{
			return JsonConvert.SerializeObject(args);
		}

		public static string SendQuick(IWebSocketConnection socket, Dictionary<string, object> args)
		{
			string jsonStr = JsonConvert.SerializeObject(args);
			socket.Send(jsonStr);
			//Console.WriteLine("Sent: " + jsonStr);
			return jsonStr;
		}

		public static void SendToAll(List<IWebSocketConnection> sockets, string message)
		{
			sockets.ForEach(socket => socket.Send(message));
		}

		public static bool VerifyName(string roomOrName)
		{
			if (roomOrName == null)
				return false;

			if (roomOrName.Trim() == "")
				return false;

			return true;
		}

		public static string GetUserName(string name, Room room, int count = 1)
		{
			var newName = name + (count != 1 ? "(" + count + ")" : "");

			if (room == null)
				return name;

			if (room.GetUser(newName) != null)
				return GetUserName(name, room, count + 1);

			return newName;
		}

		public static void VerifyFixUsername(Room room, RemoteConnectionInfo info)
		{
			var socketWithName = room.GetUser(info.Name);

			if (socketWithName != null && socketWithName != info.Socket)
			{
				info.SendSetName(GetUserName(info.Name, room), false, false);
			}
		}

		public static string IPString(IWebSocketConnection socket)
		{
			return socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort;
		}

		public static bool VerifyNameColor(string color)
		{
			// TODO: Only accept a set of colors.
			if ((color.StartsWith("#") && color.Length <= 7) || (color.StartsWith("rgb(") && color.EndsWith(")")))
				return true;

			return false;
		}

		public static string GetMD5(string url)
		{
			// Step 1, calculate MD5 hash from input
			MD5 md5 = MD5.Create();
			byte[] inputBytes = Encoding.ASCII.GetBytes(url);
			byte[] hash = md5.ComputeHash(inputBytes);

			// Step 2, convert byte array to hex string
			var sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			return sb.ToString();
		}
	}
}