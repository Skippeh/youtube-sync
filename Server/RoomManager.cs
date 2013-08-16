using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server
{
	public static class RoomManager
	{
		public static Dictionary<string, Room> Rooms { get; private set; }

		static RoomManager()
		{
			Rooms = new Dictionary<string, Room>();
		}

		public static Room CreateRoom(string name, IWebSocketConnection ownerSocket)
		{
			if (Rooms.ContainsKey(name))
				return null;

			var room = new Room(name, ownerSocket);
			Rooms.Add(name, room);

			AddUserToRoom(ownerSocket, name);

			return room;
		}

		public static bool AddUserToRoom(IWebSocketConnection socket, string roomName)
		{
			if (!Exists(roomName))
				return false;

			var info = socket.GetInfo();
			var oldRoom = info.RoomName != null ? Rooms[info.RoomName] : null;
			var newRoom = Rooms[roomName];

			if (newRoom == oldRoom)
				return true;

			if (oldRoom != null)
				RemoveUserFromRoom(socket, oldRoom.RoomName);

			// Change name if a user with this name already exists in the new room.
			Helper.VerifyFixUsername(newRoom, info);

			newRoom.AddSocket(socket);

			return true;
		}

		public static bool RemoveUserFromRoom(IWebSocketConnection socket, string roomName)
		{
			if (!Exists(roomName))
				return false;

			var info = socket.GetInfo();
			var room = info.RoomName != null ? Rooms[info.RoomName] : null;

			if (room != null)
			{
				if (!room.Sockets.Contains(socket))
					return true;

				room.RemoveSocket(socket);

				if (room.Sockets.Count == 0)
				{
					room.UpdateCurrentPlayingVideoThread.Abort();
					Rooms.Remove(room.RoomName);
				}
			}

			return true;
		}

		public static Room FindRoomByUser(IWebSocketConnection socket)
		{
			return Rooms.Values.FirstOrDefault(room => room.Sockets.Contains(socket));
		}

		public static bool Exists(string name)
		{
			if (name == null)
				return false;

			return Rooms.ContainsKey(name);
		}

		/// <summary>Removes the user from all existing rooms.</summary>
		public static void ClearUser(IWebSocketConnection socket)
		{
			var roomsCopy = new Room[Rooms.Count];
			Rooms.Values.CopyTo(roomsCopy, 0);

			foreach (var room in roomsCopy)
			{
				RemoveUserFromRoom(socket, room.RoomName);
			}
		}
	}
}