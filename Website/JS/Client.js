var Client = (function()
{
	var socket;
	var Connected;
	var Owner;
	var Privileged;
	var ClientName = "Unnamed";
	var RoomName = null;
	var ID = null;

	var CurrentVideoID = null;
	var CurrentUniqueID = null;
	var CurrentVideoState = -1;

	function Client()
	{
		
	}
});

Client.prototype.Connect = function () {
	console.log("Connecting to master server...");
	this.socket = new WebSocket(Globals.ServerLocation);

	this.socket.onopen = this.OnOpen;
	this.socket.onclose = this.OnClose;
	this.socket.onmessage = this.OnMessage;
	this.socket.onerror = this.OnError;
};

Client.prototype.Disconnect = function()
{
	if (this.socket)
	{
		try
		{
			this.socket.close();
			console.log("Client socket closed.");
		} catch(error)
		{
			console.log(error);
		}

		client.Connected = false;
		SetError("Not connected to master server.");
	}
};

Client.prototype.Send = function(dict, force)
{
	if (force == null) force = true;
	
	if (this.Connected || force)
	{
		this.socket.send(JSON.stringify(dict));
	}
};

Client.prototype.OnOpen = function (event)
{
	console.log("OnOpen, object object follows:");
	console.log(event);

	var room = queryString("room");
	
	if (name == "")
		name = CookiesGet("name");
	if (room == "")
		room = CookiesGet("room");

	client.Send({
		intent: "connect",
		name: name,
		room: room
	});
};

Client.prototype.OnClose = function(event)
{
	console.log("OnClose, event object follows:");
	console.log(event);

	var reason = "dc code " + event.code;
	ShowOverlay("Lost connection to the master server. (reason: " + reason + ")");
	client.Connected = false;
};

Client.prototype.OnMessage = function(event)
{
	console.log("OnMessage, event object follows:");
	console.log(event);

	var data = $.parseJSON(event.data);

	switch (data.intent)
	{
		case "chat":
			{
				ChatWrite(data.message, data.color, data.name, data.nameColor);

				break;
			}
		case "connectResult":
			{
				if (!data.success)
				{
					window.client.Disconnect();
					clearTimeout(window.failTimeout);
					ShowOverlay("Not allowed to connect, reason: " + data.reason);
					break;
				}

				// On success
				clearTimeout(window.failTimeout);
				HideOverlay();

				ChatWriteMotd();

				client.ClientName = data.myName;
				client.Connected = true;
				client.ID = data.id;
				break;
			}
		case "newRoom":
			{
				ChatClear();
				PlaylistClear();
				UserListClear();

				for (var i = 0; i < data.history.length; ++i)
				{
					ChatWrite(data.history[i], "rgb(107, 107, 107)");
				}

				if (data.print)
					ChatWrite(data.message, data.color);

				client.SetOwner(data.owner);
				client.SetPrivileged(false);
				client.RoomName = data.roomName;
				
				RoomListUpdate();

				UserListClear();
				for (var i = 0; i < data.users.length; ++i)
				{
					UserListAdd(data.users[i][0], data.users[i][1]);
				}

				break;
			}
		case "newName":
			{
				client.ClientName = data.newName;
				if (data.permanent)
				{
					$("#settingsNameBox").val(data.newName);
					CookiesSet("name", data.newName);
				}
				break;
			}
		case "setVideo":
			{
				client.CurrentVideoID = data.videoId;
				client.CurrentUniqueID = data.uniqueId;

				YTSetVideo(data.videoId, data.elapsed);
				YTSetState(data.state);
				if (data.message != undefined)
					ChatWrite(data.message, data.color);

				PlaylistSetCurrentInfo(data.title, "http://i2.ytimg.com/vi/" + data.videoId + "/hqdefault.jpg", TextFormatting.Linkify(data.description).replace(/\n/g, "<br/>"));
				PlaylistRemove(data.uniqueId);

				break;
			}
		case "setVideoState":
			{
				client.CurrentVideoState = data.state;
				YTSetState(data.state);
				if (data.elapsed != -1)
					YTSeek(data.elapsed);
				break;
			}
		case "videoMessage":
			{
				SetVideoURLError(data.message);
				break;
			}
		case "disconnect":
			{
				client.Disconnect();
				ShowOverlay(data.message, data.color);
				break;
			}
		case "updateOwnership":
			{
				client.SetOwner(data.owner);
				break;
			}
		case "syncVideo":
			{
				if (Math.abs(YTGetElapsed() - data.elapsed) > parseFloat(CookiesGet("MaxDesync", "3")))
					YTSeek(data.elapsed);
				
				if (YTGetState() == 1)
					YTSetState(data.state);

				break;
			}
		case "addVideo":
			{
				PlaylistAdd(data.title, data.author, data.length, data.videoId, data.uniqueId, data.channelImage);

				break;
			}
		case "getPublicRooms":
			{
				RoomListClear();

				for (var i = 0; i < data.rooms.length; ++i)
				{
					RoomListAdd(data.rooms[i][0], data.rooms[i][1]);
				}

				break;
			}
		case "addUser":
			{
				UserListAdd(data.id, data.name);
				break;
			}
		case "removeUser":
			{
				UserListRemove(data.id);
				break;
			}
		case "updatePrivileged":
			{
				client.SetPrivileged(data.privileged);
				break;
			}
		default:
			{
				console.log("Unhandled intent: " + data.intent);
			}
	}
};

Client.prototype.OnError = function(event)
{
	console.log("OnError, event object follows:");
	console.log(event);
	ShowOverlay("WebSocket error! Check console output for info.");

	client.Connected = this.readyState == 1;
};

Client.prototype.SendNewState = function(state)
{
	client.Send({
		intent: "setVideoState",
		state: state,
		elapsed: YTGetElapsed()
	});
};

Client.prototype.SetNameColorFromDiv = function(div)
{
	var jq = $(div); // jq = jquery object of div
	client.Send({
		intent: "setNameColor",
		color: jq.css("background-color")
	});
	
	$("#nameColorChooser .selected").removeClass("selected");
	jq.addClass("selected");
};

Client.prototype.SetOwner = function (isOwner)
{
	client.Owner = isOwner;
	
	var button = $("#roomSettingsButton");
	if (isOwner)
	{
		button.animate({ opacity: "1" }, 500);
		button.css("cursor", "");
	}
	else
	{
		button.animate({ opacity: "0.25" }, 500);
		button.css("cursor", "auto");
	}
}
Client.prototype.SetPrivileged = function (privileged)
{
	client.Privileged = privileged;
}

Client.prototype.IsPrivileged = function ()
{
	return client.Owner || client.Privileged;
}