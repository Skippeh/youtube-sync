using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using Server.ChatCommands;

namespace Server
{
	public static class CommandManager
	{
		private static readonly Dictionary<string, ChatCommand> chatCommands = new Dictionary<string, ChatCommand>();

		public static void Initialize()
		{
			AddListener(new NameCommand());
			AddListener(new HelpCommand());
			AddListener(new RoomCommand());
			AddListener(new UsersCommand());
			AddListener(new SetNextOwnerCommand());
			AddListener(new PlaylistCommand());
			AddListener(new TogglePrivilegedUserCommand());
			AddListener(new KickCommand());
		}

		public static bool HandleCommand(string command, IWebSocketConnection socket)
		{
			if (!command.StartsWith("/") || command.Trim() == "/")
				return false;

			var args = command.Substring(command.EndIndexOf("/ ")).Split(' ');
			if (args.Length > 1)
				args = args.Skip(1).ToArray();
			else
				args = new string[] {};

			var commandName = command.Split(' ')[0];

			commandName = commandName.Substring(1).ToLower();

			if (chatCommands.ContainsKey(commandName))
			{
				if (args.Length >= chatCommands[commandName].MinimumArgCount)
				{
					chatCommands[commandName].Args = args;
					var info = socket.GetInfo();
					var room = info != null ? info.GetRoom() : null;
					chatCommands[commandName].HandleCommand(socket, info, room);
				}
				else
				{
					socket.GetInfo().SendChatMessage("Not enough arguments: " + chatCommands[commandName].NotEnoughArgumentsString, Helper.ServerErrorColor);
				}

				return true;
			}

			return false;
		}

		public static void AddListener(ChatCommand handler)
		{
			if (handler == null)
				throw new ArgumentNullException("handler");

			var command = handler.Command.ToLower();

			if (chatCommands.ContainsKey(command))
				chatCommands[command] = handler;
			else
				chatCommands.Add(command, handler);
		}

		public static void RemoveListener(string command)
		{
			if (chatCommands.ContainsKey(command))
				chatCommands.Remove(command);
		}

		public static ChatCommand GetCommand(string command)
		{
			if (chatCommands.ContainsKey(command))
				return chatCommands[command];

			return null;
		}

		public static ChatCommand[] GetCommands()
		{
			return chatCommands.Values.ToArray();
		}
	}
}