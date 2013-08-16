using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class PlayingVideo
	{
		public string VideoID { get; set; }
		public double ElapsedSeconds { get; set; }

		public PlayState PlayState { get; set; }

		public PlayingVideo(string videoId, int startTime = 0)
		{
			VideoID = videoId;
			ElapsedSeconds = startTime;
			PlayState = PlayState.Playing;
		}
	}
}