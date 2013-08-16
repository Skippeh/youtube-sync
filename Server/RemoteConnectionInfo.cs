using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server
{
	public class RemoteConnectionInfo
	{
		public IWebSocketConnection Socket { get; private set; }

		public string Name { get; set; }

		public string NameColor { get; set; }

		public string RoomName { get; set; }

		public string ID { get; set; }

		public RemoteConnectionInfo(string name, IWebSocketConnection socket, string id)
		{
			Name = name;
			Socket = socket;
			NameColor = "rgb(117, 117, 117)";
			ID = id;
		}

		public Room GetRoom()
		{
			if (RoomManager.Exists(RoomName))
				return RoomManager.Rooms[RoomName];

			return null;
		}

		public string SendChatMessage(string message, string color = Helper.ServerMessageColor, string name = null, string nameColor = null)
		{
			var jsonStr = Helper.SendQuick(Socket, new Dictionary<string, object>
			                                       {
				                                       {"intent", "chat"},
				                                       {"message", message},
				                                       {"color", color},
				                                       {"name", name},
				                                       {"nameColor", nameColor},
			                                       });

			return jsonStr;
		}

		public void SendSetName(string name, bool print, bool permanent)
		{
			Helper.SendQuick(Socket, new Dictionary<string, object>
			                         {
				                         {"intent", "newName"},
				                         {"newName", name},
				                         {"permanent", permanent},
			                         });

			Name = name;
		}

		public void SendDisconnect(string message, string color = "#FFF")
		{
			Helper.SendQuick(Socket, new Dictionary<string, object>
			                         {
				                         {"intent", "disconnect"},
				                         {"message", message},
				                         {"color", color}
			                         });

			Socket.Close();
		}

		public void SendVideoMessage(string message)
		{
			Helper.SendQuick(Socket, new Dictionary<string, object>
			                         {
				                         {"intent", "videoMessage"},
				                         {"message", message},
			                         });
		}

		public void SendSetVideo(VideoInfo videoInfo, PlayState state, double elapsed = 0)
		{
			var info = Socket.GetInfo();

			string videoName = YoutubeHelper.GetTitle(videoInfo.VideoID);
			TimeSpan videoDuration = YoutubeHelper.GetDuration(videoInfo.VideoID);

			var title = YoutubeHelper.GetTitle(videoInfo.VideoID);
			var description = YoutubeHelper.GetDescription(videoInfo.VideoID);

			Helper.SendQuick(Socket, new Dictionary<string, object>
			                         {
				                         {"intent", "setVideo"},
				                         {"uniqueId", videoInfo.ID},
				                         {"videoId", videoInfo.VideoID},
				                         {"videoName", videoName},
				                         {"title", title},
				                         {"description", description},
				                         {"duration", (int) videoDuration.TotalSeconds},
				                         {"elapsed", elapsed},
				                         {"state", (int) state},
			                         });
		}

		public void SendSetOwner(bool isOwner = true)
		{
			Helper.SendQuick(Socket, new Dictionary<string, object>
			                         {
				                         {"intent", "updateOwnership"},
				                         {"owner", isOwner},
			                         });
		}

		public void SendAddVideo(VideoInfo videoInfo, string title, string totalTime, string author, string channelImageUrl)
		{
			var info = Socket.GetInfo();

			Helper.SendQuick(Socket, new Dictionary<string, object>
			                         {
				                         {"intent", "addVideo"},
				                         {"uniqueId", videoInfo.ID},
				                         {"videoId", videoInfo.VideoID},
				                         {"title", title},
				                         {"length", totalTime},
				                         {"author", author},
				                         {"channelImage", channelImageUrl},
			                         });
		}

		public void SendRoomPlaylist()
		{
			var room = Socket.GetInfo().GetRoom();

			if (room == null)
				return;

			foreach (var videoInfo in room.Playlist)
			{
				SendAddVideo(videoInfo, YoutubeHelper.GetTitle(videoInfo.VideoID),
				             YoutubeHelper.GetDuration(videoInfo.VideoID).ToString(),
				             YoutubeHelper.GetAuthor(videoInfo.VideoID),
				             YoutubeHelper.GetChannelImage(videoInfo.VideoID));
			}
		}

		private List<object[]> publicRoomsCache;
		private DateTime lastPublicRoomRequestTime;

		public void SendPublicRooms()
		{
			if (publicRoomsCache == null || RoomManager.Rooms.Values.Count <= Globals.MaxAllowedRoomsBeforeCache || (DateTime.Now - lastPublicRoomRequestTime).TotalMinutes >= Globals.PublicRoomsCacheExpireTime)
			{
				var rooms = new List<object[]>();

				foreach (var room2 in RoomManager.Rooms.Values)
				{
					rooms.Add(new object[] {room2.RoomName, room2.Sockets.Count});
				}

				publicRoomsCache = rooms;
				lastPublicRoomRequestTime = DateTime.Now;

				Helper.SendQuick(Socket, new Dictionary<string, object>
				                         {
					                         {"intent", "getPublicRooms"},
					                         {"rooms", rooms},
				                         });
			}
			else
			{
				Helper.SendQuick(Socket, new Dictionary<string, object>
				                         {
					                         {"intent", "getPublicRooms"},
					                         {"rooms", publicRoomsCache},
				                         });
				Console.WriteLine("Rooms cache up to date");
			}
		}

		public void SendAddUser(IWebSocketConnection user)
		{
			var info = user.GetInfo();

			Helper.SendQuick(Socket, new Dictionary<string, object>
			                         {
				                         {"intent", "addUser"},
				                         {"name", info.Name},
				                         {"id", info.ID},
			                         });
		}

		public void SendRemoveUser(IWebSocketConnection user)
		{
			var info = user.GetInfo();

			Helper.SendQuick(Socket, new Dictionary<string, object>
			                         {
				                         {"intent", "removeUser"},
				                         {"id", info.ID},
			                         });
		}

		public void SendAndSetPrivileged(bool privileged)
		{
			var room = GetRoom();

			if (room != null)
			{
				if (privileged)
				{
					room.AddPrivilegedUser(Socket);
				}
				else
				{
					room.RemovePrivilegedUser(Socket);
				}

				Helper.SendQuick(Socket, new Dictionary<string, object>
				                         {
					                         {"intent", "updatePrivileged"},
					                         {"privileged", privileged},
				                         });

				SendChatMessage(privileged
					                ? "You are now a privileged user."
					                : "You are no longer a privileged user.", Helper.ServerRoomColor);
			}
		}
	}
}