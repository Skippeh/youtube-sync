using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace Server
{
	public static class SocketConnectionExtensions
	{
		private static readonly Dictionary<IWebSocketConnection, RemoteConnectionInfo> tags = new Dictionary<IWebSocketConnection, RemoteConnectionInfo>();

		public static void SetInfo(this IWebSocketConnection socket, RemoteConnectionInfo info)
		{
			if (tags.ContainsKey(socket))
				tags[socket] = info;
			else
				tags.Add(socket, info);
		}

		public static RemoteConnectionInfo GetInfo(this IWebSocketConnection socket)
		{
			if (tags.ContainsKey(socket))
				return tags[socket];

			return null;
		}
	}
}