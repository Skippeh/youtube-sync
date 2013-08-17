using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public static class Globals
	{
		/// <summary>The time in minutes a cached video info is considered up to date.</summary>
		public static int CacheExpireTime { get; private set; }

		/// <summary>The time in minutes the cached info about all rooms is considered up to date.</summary>
		public static int PublicRoomsCacheExpireTime { get; private set; }

		/// <summary>The total amount of rooms allowed before we start caching the information about rooms.</summary>
		public static int MaxAllowedRoomsBeforeCache { get; private set; }

		/// <summary>The location of this server as a websocket url.</summary>
		public static string MyLocation { get; private set; }

		public static string GoogleAPIKey { get; private set; } // Keep this secret!

		public static void Initialize()
		{
			var saveFile = new SaveFile("settings.json");

			try
			{
				CacheExpireTime = (int)saveFile.Get<long>("CacheExpireTime", (long)30);
				PublicRoomsCacheExpireTime = (int)saveFile.Get<long>("PublicRoomsCacheExpireTime", (long)5);
				MaxAllowedRoomsBeforeCache = (int)saveFile.Get<long>("MaxAllowedRoomsBeforeCache", (long)20);
				MyLocation = saveFile.Get<string>("ServerLocation", "ws://YourServerLocation");
				GoogleAPIKey = saveFile.Get<string>("GoogleAPIKey", "");
				saveFile.SaveKeyValues();
			}
			catch (InvalidCastException)
			{
				Console.WriteLine("Failed to deserialize settings!!");
				Environment.Exit(2);
			}
		}
	}
}