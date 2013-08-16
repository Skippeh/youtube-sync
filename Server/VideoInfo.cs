using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class VideoInfo
	{
		public string ID { get; private set; }
		public string VideoID { get; private set; }
		public int StartTime { get; private set; }

		public VideoInfo(string videoId, string id, int startTime)
		{
			ID = id;
			VideoID = videoId;
			StartTime = startTime;
		}
	}
}