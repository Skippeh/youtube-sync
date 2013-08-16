using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server.Packets
{
	public class AddVideoPacket : Packet
	{
		public AddVideoPacket() : base("addVideo", "id") {}

		public override void HandlePacket(Dictionary<string, object> data, IWebSocketConnection socket, RemoteConnectionInfo info, Room room, ref List<IWebSocketConnection> allSockets)
		{
			string id = data["id"] as string;
			int startSeconds = 0;

			if (data.ContainsKey("t"))
				startSeconds = (int) (long) data["t"];

			if (room != null)
			{
				var videoInfo = room.AddVideo(id, startSeconds);

				if (YoutubeHelper.VideoExists(id))
				{
					if (YoutubeHelper.CanEmbed(id))
					{
						var title = YoutubeHelper.GetTitle(id);
						var author = YoutubeHelper.GetAuthor(id);
						var channelImage = YoutubeHelper.GetChannelImage(id);
						var totalSeconds = YoutubeHelper.GetTotalTime(id);

						var totalTimeSpan = TimeSpan.FromSeconds(totalSeconds);
						var totalTime = string.Format("({0})", totalTimeSpan.ToString());

						room.SendAddVideoToAll(videoInfo, title, totalTime, author, channelImage);

						room.SendChatMessageToAll(info.Name + " added " + title + " to the playlist.");

						if (room.CurrentPlayingVideo == null)
						{
							room.GotoNextVideo();
							room.PlayVideo();
							room.SendSetVideoToAll(room.CurrentPlayingVideo.VideoID, room.CurrentPlayingVideo.PlayState, (float) room.CurrentPlayingVideo.ElapsedSeconds);
						}
					}
					else
					{
						info.SendVideoMessage("Author of video doesn't allow embedding this video.");
					}
				}
				else
				{
					info.SendVideoMessage("Video does not exist on youtube.");
				}
			}
			else
			{
				info.SendVideoMessage("You need to join a room first.");
			}
		}
	}
}