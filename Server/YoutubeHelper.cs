using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using Server.DBTypes;

namespace Server
{
	public static class YoutubeHelper
	{
		private static WebClient webClient = new WebClient();

		//private static Dictionary<string, CachedVideoInfo> cachedInfos = new Dictionary<string, CachedVideoInfo>();

		private static SQLiteConnection dbConnection;

		public static void Initialize()
		{
			webClient.Headers.Add("X-GData-Key", "key=" + Globals.GoogleAPIKey);
			dbConnection = new SQLiteConnection("Database.sqlite", SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create, true);

			dbConnection.Execute(@"
CREATE TABLE IF NOT EXISTS Cache
(
	ID int PRIMARY KEY ASC,
	VideoID text,
	ApiInfo text,
	Date int
)");
		}

		public static bool VideoExists(string videoId)
		{
			try
			{
				VerifyCacheData(videoId);
				return true;
			}
			catch (WebException)
			{
				return false;
			}
		}

		/// <summary>This method assumes the video id is valid and that a video with that id exists!</summary>
		public static TimeSpan GetDuration(string videoId)
		{
			VerifyCacheData(videoId);

			//var info = cachedInfos[videoId];
			var info = GetCachedInfo(videoId);
			return new TimeSpan(0, 0, 0, info.ApiInfo["media$group"]["yt$duration"]["seconds"].Value<int>());
		}

		/// <summary>This method assumes the video id is valid and that a video with that id exists!</summary>
		public static string GetTitle(string videoId)
		{
			VerifyCacheData(videoId);

			//var info = cachedInfos[videoId];
			var info = GetCachedInfo(videoId);
			return info.ApiInfo["title"]["$t"].Value<string>();
		}

		/// <summary>This method assumes the video id is valid and that a video with that id exists!<para>This method returns fresh api data from the YouTube api.</para></summary>
		private static Dictionary<string, object> GetFreshData(string videoId)
		{
			return JsonConvert.DeserializeObject<Dictionary<string, object>>(GetFreshJsonString(videoId));
		}

		/// <summary>This method assumes the video id is valid and that a video with that id exists!<para/>This method returns fresh api data as a json string from the YouTube api.</summary>
		private static string GetFreshJsonString(string videoId)
		{
			Console.WriteLine("Downloading fresh data for id " + videoId + ".");
			return webClient.DownloadString((Globals.APIUseHttps ? "https" : "http") + "://gdata.youtube.com/feeds/api/videos/" + videoId + "?v=2&alt=json");
		}

		/// <summary>Verifies the local cache data, and downloads new data if no data is found, or it's expired.</summary>
		private static void VerifyCacheData(string videoId)
		{
			var cachedInfo = GetCachedInfo(videoId);
			if (cachedInfo != null && (DateTime.Now - cachedInfo.LastApiUpdate).TotalMinutes > Globals.CacheExpireTime)
			{
				var jsonString = GetFreshJsonString(videoId);
				SetCachedInfo(videoId, new CachedVideoInfo(jsonString));
			}
			else if (cachedInfo == null)
			{
				SetCachedInfo(videoId, new CachedVideoInfo(GetFreshJsonString(videoId)));
			}
		}

		private static void SetCachedInfo(string videoId, CachedVideoInfo info)
		{
			var existingInfo = dbConnection.Query<DBCacheRecord>("SELECT * FROM Cache WHERE VideoID = ?", videoId);

			if (existingInfo.Count > 0)
			{
				dbConnection.Execute("DELETE FROM Cache WHERE VideoID = ?", videoId);
			}

			dbConnection.Execute(@"
INSERT INTO Cache(VideoID, ApiInfo, Date)
VALUES(?, ?, ?)
", videoId, info.RawApiInfo, info.LastApiUpdate);
		}

		public static string GetAuthor(string videoId)
		{
			VerifyCacheData(videoId);
			var info = GetCachedInfo(videoId);

			return info.ApiInfo["author"][0]["name"]["$t"].Value<string>();
		}

		public static string GetChannelImage(string videoId)
		{
			VerifyCacheData(videoId);
			var info = GetCachedInfo(videoId);

			var channelId = info.ApiInfo["author"][0]["yt$userId"]["$t"].Value<string>();

			return "http://i4.ytimg.com/i/" + channelId + "/1.jpg";
		}

		/// <summary>Returns the videos total time in seconds.</summary>
		public static int GetTotalTime(string videoId)
		{
			VerifyCacheData(videoId);
			var info = GetCachedInfo(videoId);

			var time = info.ApiInfo["media$group"]["yt$duration"]["seconds"].Value<int>();

			return time;
		}

		public static bool CanEmbed(string videoId)
		{
			VerifyCacheData(videoId);
			var info = GetCachedInfo(videoId);

			return info.ApiInfo["yt$accessControl"][4]["permission"].Value<string>() == "allowed";
		}

		public static string GetDescription(string videoID)
		{
			VerifyCacheData(videoID);
			var info = GetCachedInfo(videoID);

			return info.ApiInfo["media$group"]["media$description"]["$t"].Value<string>();
		}

		private static CachedVideoInfo GetCachedInfo(string videoId)
		{
			var info = dbConnection.Query<DBCacheRecord>("SELECT ApiInfo,Date FROM Cache WHERE VideoID = ?", videoId);

			if (info.Count > 0)
			{
				return new CachedVideoInfo(info[0].ApiInfo, info[0].Date);
			}

			return null;
		}

		private class CachedVideoInfo
		{
			public string RawApiInfo { get; set; }
			public JObject ApiInfo { get; set; }
			public DateTime LastApiUpdate { get; set; }

			public CachedVideoInfo(string apiInfo)
				: this(apiInfo, DateTime.Now) {}

			public CachedVideoInfo(string apiInfo, DateTime lastApiUpdate)
			{
				ApiInfo = (JObject) JsonConvert.DeserializeObject<Dictionary<string, object>>(apiInfo)["entry"];
				RawApiInfo = apiInfo;

				LastApiUpdate = lastApiUpdate;
			}
		}
	}
}