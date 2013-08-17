var Client = {};

Client.socket = null;
Client.Connected = false;
Client.Owner = false;
Client.Privileged = false;
Client.ClientName = "Unnamed";
Client.RoomName = null;
Client.ID = null;

Client.CurrentVideoID = null;
Client.CurrentUniqueID = null;
Client.CurrentVideoState = -1;

Client.Connect = function () {
	console.log("Connecting to master server...");
	Client.socket = new WebSocket(Globals.ServerLocation);

	Client.socket.onopen = function (event) { Client.OnOpen(event); };
	Client.socket.onclose = function (event) { Client.OnClose(event); };
	Client.socket.onmessage = function (event) { Client.OnMessage(event); };
	Client.socket.onerror = function (event) { Client.OnError(event); };
};

Client.Disconnect = function () {
	if (Client.socket) {
		try {
			Client.socket.close();
			console.log("Client socket closed.");
		} catch (error) {
			console.log(error);
		}

		Client.Connected = false;
		SetError("Not connected to master server.");
	}
};

Client.Send = function (dict, force) {
	if (force == null) force = true;

	if (Client.Connected || force) {
		Client.socket.send(JSON.stringify(dict));
	}
	
	SetError("");
};

Client.OnOpen = function (event)
{
	console.log("OnOpen, object object follows:");
	console.log(event);

	var room = queryString("room");

	if (name == "")
		name = Cookies.Get("name");
	if (room == "")
		room = Cookies.Get("room");

	Client.Send({
		intent: "connect",
		name: name,
		room: room
	});
};

Client.OnClose = function (event) {
	console.log("OnClose, event object follows:");
	console.log(event);

	var reason = "dc code " + event.code;
	ShowOverlay("Lost connection to the master server. (reason: " + reason + ")");
	Client.Connected = false;
};

Client.OnMessage = function (event) {
	console.log("OnMessage, event object follows:");
	console.log(event);

	var data = $.parseJSON(event.data);

	switch (data.intent) {
		case "chat":
			{
				Chat.Write(data.message, data.color, data.name, data.nameColor);

				break;
			}
		case "connectResult":
			{
				if (!data.success) {
					window.Client.Disconnect();
					clearTimeout(window.failTimeout);
					ShowOverlay("Not allowed to connect, reason: " + data.reason);
					break;
				}

				// On success
				clearTimeout(window.failTimeout);
				HideOverlay();

				Chat.WriteMotd();

				Client.ClientName = data.myName;
				Client.Connected = true;
				Client.ID = data.id;
				break;
			}
		case "newRoom":
			{
				Chat.Clear();
				Playlist.Clear();
				UserList.Clear();

				for (var i = 0; i < data.history.length; ++i) {
					Chat.Write(data.history[i], "rgb(107, 107, 107)");
				}

				if (data.print)
					Chat.Write(data.message, data.color);

				Client.SetOwner(data.owner);
				Client.SetPrivileged(false);
				Client.RoomName = data.roomName;

				RoomList.Update();

				UserList.Clear();
				for (var i = 0; i < data.users.length; ++i) {
					UserList.Add(data.users[i][0], data.users[i][1]);
				}

				break;
			}
		case "newName":
			{
				Client.ClientName = data.newName;
				if (data.permanent) {
					$("#settingsNameBox").val(data.newName);
					Cookies.Set("name", data.newName);
				}
				break;
			}
		case "setVideo":
			{
				Client.CurrentVideoID = data.videoId;
				Client.CurrentUniqueID = data.uniqueId;

				YT.SetVideo(data.videoId, data.elapsed);
				YT.SetState(data.state);
				if (data.message != undefined)
					Chat.Write(data.message, data.color);

				Playlist.SetCurrentInfo(data.title, "http://i2.ytimg.com/vi/" + data.videoId + "/hqdefault.jpg", TextFormatting.Linkify(data.description).replace(/\n/g, "<br/>"));
				Playlist.Remove(data.uniqueId);

				break;
			}
		case "setVideoState":
			{
				Client.CurrentVideoState = data.state;
				YT.SetState(data.state);
				if (data.elapsed != -1)
					YT.Seek(data.elapsed);
				break;
			}
		case "videoMessage":
			{
				SetVideoURLError(data.message);
				break;
			}
		case "disconnect":
			{
				Client.Disconnect();
				ShowOverlay(data.message, data.color);
				break;
			}
		case "updateOwnership":
			{
				Client.SetOwner(data.owner);
				break;
			}
		case "syncVideo":
			{
				if (Math.abs(YT.GetElapsed() - data.elapsed) > parseFloat(Cookies.Get("MaxDesync", "3")))
					YT.Seek(data.elapsed);

				if (YT.GetState() == 1)
					YT.SetState(data.state);

				break;
			}
		case "addVideo":
			{
				Playlist.Add(data.title, data.author, data.length, data.videoId, data.uniqueId, data.channelImage);

				break;
			}
		case "getPublicRooms":
			{
				RoomList.Clear();

				for (var i = 0; i < data.rooms.length; ++i) {
					RoomList.Add(data.rooms[i][0], data.rooms[i][1]);
				}

				break;
			}
		case "addUser":
			{
				UserList.Add(data.id, data.name);
				break;
			}
		case "removeUser":
			{
				UserList.Remove(data.id);
				break;
			}
		case "updatePrivileged":
			{
				Client.SetPrivileged(data.privileged);
				break;
			}
		default:
			{
				console.log("Unhandled intent: " + data.intent);
			}
	}
};

Client.OnError = function (event) {
	console.log("OnError, event object follows:");
	console.log(event);
	ShowOverlay("WebSocket error! Check console output for info.");

	Client.Connected = Client.readyState == 1;
};

Client.SendNewState = function (state) {
	Client.Send({
		intent: "setVideoState",
		state: state,
		elapsed: YT.GetElapsed()
	});
};

Client.SetNameColorFromDiv = function (div) {
	var jq = $(div); // jq = jquery object of div
	Client.Send({
		intent: "setNameColor",
		color: jq.css("background-color")
	});

	$("#nameColorChooser .selected").removeClass("selected");
	jq.addClass("selected");
};

Client.SetOwner = function (isOwner) {
	Client.Owner = isOwner;

	var button = $("#roomSettingsButton");
	if (isOwner) {
		button.animate({ opacity: "1" }, 500);
		button.css("cursor", "");
	}
	else {
		button.animate({ opacity: "0.25" }, 500);
		button.css("cursor", "auto");
	}
};

Client.SetPrivileged = function (privileged) {
	Client.Privileged = privileged;
};

Client.IsPrivileged = function () {
	return Client.Owner || Client.Privileged;
};