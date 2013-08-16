using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.ChatCommands
{
	public class PlaylistCommand : ChatCommand
	{
		public PlaylistCommand() : base("playlist", 0) {}

		public override string NotEnoughArgumentsString
		{
			get { return ""; }
		}

		public override string HelpString
		{
			get { return "Returns a list of videos in the current rooms playlist."; }
		}

		public override void HandleCommand(Fleck.IWebSocketConnection socket, RemoteConnectionInfo info, Room room)
		{
			if (room != null)
			{
				var videoInfos = room.Playlist.ToArray();

				string message = "-- Start of playlist --\n";
				for (int i = 0; i < videoInfos.Length; i++)
				{
					var videoId = videoInfos[i];
					message += "[" + i + "]: " + videoId.VideoID + "\n";
				}
				message += "-- End of playlist --";

				info.SendChatMessage(message);
			}
		}
	}
}