using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Fleck;
using Newtonsoft.Json;

namespace Server
{
	/// <summary>
	/// Summary description for Room
	/// </summary>
	public class Room
	{
		public string RoomName { get; set; }

		public List<string> ChatHistory { get; set; }

		public List<IWebSocketConnection> Sockets { get; private set; }

		public IWebSocketConnection Owner { get; set; }
		public IWebSocketConnection NextOwner { get; set; }

		/// <summary>A list of users that are allowed to control the playlist and current video.</summary>
		public List<IWebSocketConnection> PrivilegedUsers { get; private set; }

		public VideoInfo CurrentVideo { get; set; }
		public PlayingVideo CurrentPlayingVideo { get; set; }

		public Queue<VideoInfo> Playlist { get; private set; }

		public Thread UpdateCurrentPlayingVideoThread;

		public Room(string name, IWebSocketConnection owner)
		{
			ChatHistory = new List<string>();
			Sockets = new List<IWebSocketConnection>();
			Playlist = new Queue<VideoInfo>();
			PrivilegedUsers = new List<IWebSocketConnection>();
			RoomName = name;
			Owner = owner;

			UpdateCurrentPlayingVideoThread = new Thread(UpdateCurrentVideo);
			UpdateCurrentPlayingVideoThread.Start();
		}

		public void AddSocket(IWebSocketConnection socket)
		{
			if (socket == null)
				throw new ArgumentNullException("socket");

			if (Sockets.Contains(socket))
				return;

			socket.GetInfo().RoomName = RoomName;

			Sockets.Add(socket);

			Helper.SendQuick(socket, new Dictionary<string, object>
			                         {
				                         {"intent", "newRoom"},
				                         {"message", "Changed room to " + RoomName + "."},
				                         {"color", Helper.ServerMessageColor},
				                         {"history", ChatHistory},
				                         {"owner", socket == Owner},
				                         {"roomName", RoomName},
				                         {
					                         "users", Sockets.Select(s =>
					                                                 {
						                                                 var info = s.GetInfo();
						                                                 return new object[] {info.ID, info.Name};
					                                                 }).ToArray()
				                         },
			                         });

			SendUserToAll(socket);

			if (CurrentPlayingVideo != null)
			{
				socket.GetInfo().SendSetVideo(CurrentVideo, CurrentPlayingVideo.PlayState, CurrentPlayingVideo.ElapsedSeconds);
			}

			socket.GetInfo().SendRoomPlaylist();

			SendChatMessageToAll(socket.GetInfo().Name + " joined the room. (" + RoomName + ")");
		}

		public void RemoveSocket(IWebSocketConnection socket)
		{
			if (!Sockets.Contains(socket))
				return;

			socket.GetInfo().RoomName = null;

			Sockets.Remove(socket);

			SendRemoveUserToAll(socket);
			SendChatMessageToAll(socket.GetInfo().Name + " left the room.");

			if (socket == Owner)
			{
				var newOwner = NextOwner ?? Sockets.FirstOrDefault();
				if (newOwner != null)
				{
					Owner = newOwner;
					newOwner.GetInfo().SendSetOwner();
					newOwner.GetInfo().SendChatMessage("You are now the proud owner of " + RoomName + "!", Helper.ServerRoomColor);
				}
			}
			else if (socket == NextOwner)
			{
				NextOwner = null;
				Owner.GetInfo().SendChatMessage("Caution: the next owner left the room.", Helper.ServerRoomColor);
			}
		}

		/// <summary>Returns the json string that this chat message generates.</summary>
		public string SendChatMessageToAll(string message, string name = null, string color = Helper.ServerMessageColor, string nameColor = Helper.DefaultNameColor)
		{
			if (!Sockets.Any())
				return null;

			string jsonStr = null;
			Sockets.ForEach(socket => jsonStr = socket.GetInfo().SendChatMessage(message, color, name, nameColor));

			var prefix = name != null ? (name + ": ") : "";
			ChatHistory.Add(prefix + message);

			return jsonStr;
		}

		public void SendVideoMessageToAll(string message)
		{
			Sockets.ForEach(socket => socket.GetInfo().SendVideoMessage(message));
		}

		public void SendToAll(Dictionary<string, object> args)
		{
			if (!Sockets.Any())
				return;

			var jsonStr = JsonConvert.SerializeObject(args);
			Sockets.ForEach(socket => socket.Send(jsonStr));
		}

		public void SendToAllExcept(IWebSocketConnection except, Dictionary<string, object> args)
		{
			if (!Sockets.Any() || (Sockets.Count == 1 && Sockets[0] == except))
				return;

			var jsonStr = JsonConvert.SerializeObject(args);
			Sockets.ForEach(socket =>
			                {
				                if (socket == except)
					                return;

				                socket.Send(jsonStr);
			                });
		}

		public VideoInfo AddVideo(string videoId, int startTime = 0)
		{
			var videoInfo = new VideoInfo(videoId, GetUniqueId(videoId), startTime);
			Playlist.Enqueue(videoInfo);
			return videoInfo;
		}

		public string[] GetPlaylist()
		{
			return Playlist.Select(info => info.VideoID).ToArray();
		}

		/// <summary>Proceed to the next video. Returns the new video's video id, or null if the playlist is empty.</summary>
		public string GotoNextVideo()
		{
			if (Playlist.Count >= 1)
			{
				var nextVideo = Playlist.Dequeue();
				CurrentVideo = nextVideo;
				CurrentPlayingVideo = new PlayingVideo(CurrentVideo.VideoID, CurrentVideo.StartTime);
				return nextVideo.VideoID;
			}
			else
				CurrentPlayingVideo = null;

			return null;
		}

		public bool PlayVideo()
		{
			if (CurrentPlayingVideo == null)
				GotoNextVideo();

			if (CurrentPlayingVideo == null) // Playlist queue empty
				return false;

			CurrentPlayingVideo.PlayState = PlayState.Playing;

			Console.WriteLine("[{0}] Video unpaused.", RoomName);

			return true;
		}
		
		public void PauseVideo()
		{
			if (CurrentPlayingVideo == null || CurrentPlayingVideo.PlayState != PlayState.Playing)
				return;

			CurrentPlayingVideo.PlayState = PlayState.Paused;
			Console.WriteLine("[{0}] Video paused.", RoomName);
		}

		private string GetUniqueId(string url)
		{
			return Helper.GetMD5(url + DateTime.Now.ToLongTimeString());
		}

		private void UpdateCurrentVideo()
		{
			bool sync = false;

			while (true)
			{
				if (CurrentPlayingVideo != null && CurrentPlayingVideo.PlayState != PlayState.Paused)
				{
					CurrentPlayingVideo.ElapsedSeconds += 1;
					//Console.WriteLine("[{0}] Elapsed Seconds: {1}.", RoomName, CurrentPlayingVideo.ElapsedSeconds);

					if (sync)
					{
						sync = false;

						SendSyncRoomToAll();
					}
					else
						sync = true;
				}

				Thread.Sleep(1000);
			}
		}

		private void SendSyncRoomToAll()
		{
			Sockets.ForEach(socket =>
			                {
				                if (socket != Owner)
					                SendSyncRoom(socket);
			                });
		}

		public bool IsOwner(IWebSocketConnection socket)
		{
			return Owner == socket;
		}

		public IWebSocketConnection GetUser(string name)
		{
			return Sockets.FirstOrDefault(socket => socket.GetInfo().Name.ToLower() == name.ToLower());
		}

		public void SendSyncRoom(IWebSocketConnection socket)
		{
			if (CurrentPlayingVideo == null)
				return;

			Helper.SendQuick(socket, new Dictionary<string, object>
			                         {
				                         {"intent", "syncVideo"},
				                         {"state", (int) CurrentPlayingVideo.PlayState},
				                         {"elapsed", CurrentPlayingVideo.ElapsedSeconds},
			                         });
		}

		public void SendSetVideoToAll(string videoId, PlayState state, float elapsed = 0)
		{
			Sockets.ForEach(socket => socket.GetInfo().SendSetVideo(CurrentVideo, state, elapsed));
		}

		public void SendSetVideoToAll()
		{
			Sockets.ForEach(socket => socket.GetInfo().SendSetVideo(CurrentVideo, CurrentPlayingVideo.PlayState, CurrentPlayingVideo.ElapsedSeconds));
		}

		public void SendAddVideoToAll(VideoInfo videoInfo, string title, string totalTime, string author, string channelImageUrl)
		{
			Sockets.ForEach(socket => socket.GetInfo().SendAddVideo(videoInfo, title, totalTime, author, channelImageUrl));
		}

		public void SendUserToAll(IWebSocketConnection user)
		{
			Sockets.ForEach(socket => socket.GetInfo().SendAddUser(user));
		}

		public void SendRemoveUserToAll(IWebSocketConnection user)
		{
			Sockets.ForEach(socket => socket.GetInfo().SendRemoveUser(user));
		}

		public void AddPrivilegedUser(IWebSocketConnection user)
		{
			if (Sockets.Contains(user) && !PrivilegedUsers.Contains(user) && Owner != user)
			{
				PrivilegedUsers.Add(user);
			}
		}

		public void RemovePrivilegedUser(IWebSocketConnection user)
		{
			if (PrivilegedUsers.Contains(user))
			{
				PrivilegedUsers.Remove(user);
			}
		}

		public bool IsPrivileged(IWebSocketConnection user)
		{
			return Owner == user || PrivilegedUsers.Contains(user);
		}

		public void KickUser(IWebSocketConnection user)
		{
			if (Sockets.Contains(user))
			{
				RemoveSocket(user);
				user.GetInfo().SendChatMessage("You have been kicked from the room.", Helper.ServerErrorColor);
			}

			SendChatMessageToAll(user.GetInfo().Name + " was kicked from the room.");
		}
	}
}