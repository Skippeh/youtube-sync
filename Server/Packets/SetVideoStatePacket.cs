using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.Packets
{
	public class SetVideoStatePacket : Packet
	{
		public SetVideoStatePacket() : base("setVideoState", "state", "elapsed") {}

		public override void HandlePacket(Dictionary<string, object> data, IWebSocketConnection socket, RemoteConnectionInfo info, Room room, ref List<IWebSocketConnection> allSockets)
		{
			if (room == null)
				return;

			if (!room.IsPrivileged(socket))
				return;

			var state = (long) data["state"];

			if (room.CurrentPlayingVideo == null)
			{
				info.SendVideoMessage("Could not edit state of video to " + state + " because the video doesn't exist.");
				return;
			}

			if ((int)room.CurrentPlayingVideo.PlayState != state)
			{
				string action = null;
				
				Console.WriteLine("[{0}] Elapsed Time Set: {1}", room.RoomName, data["elapsed"]);

				switch (state)
				{
					case 0:
					{
						action = "finished";
						room.GotoNextVideo();
						if (room.PlayVideo())
						{
							room.SendSetVideoToAll();
						}
						break;
					}
					case 1:
					{
						action = "started";
						room.PlayVideo();
						break;
					}
					case 2:
					{
						action = "paused";
						room.PauseVideo();
						break;
					}
					case 3:
					{
						action = "is buffering";
						room.PauseVideo();
						break;
					}
					case 5:
					{
						action = "stopped";
						room.GotoNextVideo();
						if (room.PlayVideo())
							room.SendSetVideoToAll();
						break;
					}
				}

				if (room.CurrentPlayingVideo != null) // CurrentPlayingVideo is null if playlist is empty.
				{
					// In the case that the elapsed seconds is an integer, the value will be parsed as a long.
					if (data["elapsed"] is long)
						room.CurrentPlayingVideo.ElapsedSeconds = (long)data["elapsed"];
					else
						room.CurrentPlayingVideo.ElapsedSeconds = (double)data["elapsed"];

					room.SendToAllExcept(socket, new Dictionary<string, object>
					                             {
						                             {"intent", "setVideoState"},
						                             {"state", data["state"]},
						                             {"elapsed", room.CurrentPlayingVideo.ElapsedSeconds},
					                             });

					//room.SendVideoMessageToAll(info.Name + " " + action + " the video.");
				}
			}
		}
	}
}