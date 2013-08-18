Chat = {};

Chat.ChatHistory = [];
Chat.HistoryPosition = 0;

Chat.WriteMotd = function ()
{
	var chat = $("#chatText");

	chat.append("<span style=\"color:rgb(180, 180, 180);\"><u>Never tell your password to anyone.</u></span>");
	if (Client.RoomName == null)
		chat.append("<br/><span style=\"color:rgb(180, 180, 180);\">Type /room [room name] to begin.</span><br/>");
	else
		chat.append("<br/>");
};

Chat.Write = function (text, color, name, nameColor)
{
	var originalText = text;
	var originalName = name;
	text = Chat.SecureText(text);
	text = TextFormatting.FormatText(text);
	
	var chat = $("#chatText");
	
	if (name != undefined)
	{
		name = Chat.SecureText(name);
		
		if (name == Client.ClientName && color == "#FFF")
			color = "#BBB";

		chat.append("<span style=\"color:" + nameColor + ";font-weight:bold;\">" + name + "</span>: <span style=\"color:" + color + ";\">" + text + "</span>");
	}
	else
	{
		chat.append("<span style=\"color:" + color + ";\">" + text + "</span>");
	}

	chat.append("<br/>");

	Chat.ScrollToBottom();
	
	if (name != undefined) // It's a user submitted chat message not send by myself.
	{
		if (Cookies.Get("ChatSounds", "1") == "1" && !WindowFocused)
		{
			document.getElementById("audioMessageReceived").play();
		}

		if (Cookies.Get("ChatNotifications", "1") == "1" && !WindowFocused && window.webkitNotifications)
		{
			console.log("Showing notification: " + webkitNotifications.checkPermission());

			if (webkitNotifications.checkPermission() == 0)
			{
				var notification = webkitNotifications.createNotification("", originalName + " sent a message", originalText);

				notification.onclick = function()
				{
					window.focus();
					this.cancel();
					$("#chatInputBox").focus();
				};

				setTimeout(function() {
					notification.close();
				}, 6000);

				notification.show();
			}
		}
	}
	
	$(".scrollbar").TrackpadScrollEmulator("recalculate");
};

Chat.SecureText = function(text)
{
	text = text.replace(/</g, "&lt;");
	text = text.replace(/>/g, "&gt;");

	return text;
};

Chat.ClearInput = function()
{
	$("#chatInputBox").val("");
};

Chat.Clear = function()
{
	$("#chatText").text("");
	Chat.WriteMotd();
	$(".scrollbar").TrackpadScrollEmulator("recalculate");
};

Chat.ScrollToBottom = function()
{
	var chatDiv = $("#rightSidebar .tse-scroll-content");
	chatDiv.animate(
		{
			scrollTop: chatDiv[0].scrollHeight - chatDiv.height()
		}, 0); // 0 ms to animate to this state. (i know)
};

Chat.AddHistory = function(message)
{
	Chat.ChatHistory.unshift(message);
};

Chat.GetHistory = function(direction)
{
	if (Chat.ChatHistory.length == 0)
		return "";

	if (direction == "up")
	{
		if (Chat.HistoryPosition + 1 < Chat.ChatHistory.length)
			Chat.HistoryPosition += 1;
	}
	else if (direction == "down")
	{
		if (Chat.HistoryPosition - 1 >= 0)
			Chat.HistoryPosition -= 1;
		else
		{
			Chat.ResetHistoryPos();
			return "";
		}
	}

	return Chat.ChatHistory.slice()[Chat.HistoryPosition];
};

Chat.ResetHistoryPos = function()
{
	Chat.HistoryPosition = -1;
};

Number.prototype.formatDate = function()
{
	if (this < 10)
		return "0" + this;

	return this.toString();
};