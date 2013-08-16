using System;
using System.Collections.Generic;
using Fleck;

namespace Server
{
	public abstract class Packet
	{
		public string Intent { get; private set; }

		/// <summary>This packet will only be handled if the incoming data contains the keys this array holds.</summary>
		public string[] RequiredParams { get; private set; }

		protected Packet(string intent, params string[] requiredParams)
		{
			if (intent == null)
				throw new ArgumentNullException("intent");

			Intent = intent;
			RequiredParams = requiredParams;
		}

		public abstract void HandlePacket(Dictionary<string, object> data, IWebSocketConnection socket, RemoteConnectionInfo info, Room room, ref List<IWebSocketConnection> allSockets);
	}
}