var ChatHistory = [];
var ChatHistoryPosition;

function ChatWriteMotd()
{
	var chat = $("#chatText");

	chat.append("<span style=\"color:rgb(180, 180, 180);\"><u>Never tell your password to anyone.</u></span>");
	if (client.RoomName == null)
		chat.append("<br/><span style=\"color:rgb(180, 180, 180);\">Type /room [room name] to begin.</span><br/>");
	else
		chat.append("<br/>");
}

function ChatWrite(text, color, name, nameColor)
{
	var originalText = text;
	var originalName = name;
	text = ChatSecureText(text);
	text = TextFormatting.FormatText(text);
	
	var chat = $("#chatText");
	
	if (name != undefined)
	{
		name = ChatSecureText(name);
		
		if (name == client.ClientName && color == "#FFF")
			color = "#BBB";

		chat.append("<span style=\"color:" + nameColor + ";font-weight:bold;\">" + name + "</span>: <span style=\"color:" + color + ";\">" + text + "</span>");
	}
	else
	{
		chat.append("<span style=\"color:" + color + ";\">" + text + "</span>");
	}

	chat.append("<br/>");

	ChatScrollToBottom();
	
	if (name != undefined) // It's a user submitted chat message not send by myself.
	{
		if (CookiesGet("ChatSounds", "1") == "1" && !WindowFocused)
		{
			document.getElementById("audioMessageReceived").play();
		}

		if (CookiesGet("ChatNotifications", "1") == "1" && !WindowFocused && window.webkitNotifications)
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
}

function ChatSecureText(text)
{
	text = text.replace(/</g, "&lt;");
	text = text.replace(/>/g, "&gt;");

	return text;
}

function ChatClearInput()
{
	$("#chatInputBox").val("");
}

function ChatClear()
{
	$("#chatText").text("");
	ChatWriteMotd();
}

function ChatScrollToBottom()
{
	var chatDiv = $("#chat");
	chatDiv.animate(
		{
			scrollTop: chatDiv[0].scrollHeight - chatDiv.height()
		}, 0); // 0 ms to animate to this state. (i know)
}

function ChatAddHistory(message)
{
	ChatHistory.unshift(message);
}

function ChatGetHistory(direction)
{
	if (ChatHistory.length == 0)
		return "";

	if (direction == "up")
	{
		if (ChatHistoryPosition + 1 < ChatHistory.length)
			ChatHistoryPosition += 1;
	}
	else if (direction == "down")
	{
		if (ChatHistoryPosition - 1 >= 0)
			ChatHistoryPosition -= 1;
		else
		{
			ChatResetHistoryPos();
			return "";
		}
	}

	return ChatHistory.slice()[ChatHistoryPosition];
}

function ChatResetHistoryPos()
{
	ChatHistoryPosition = -1;
}

Number.prototype.formatDate = function()
{
	if (this < 10)
		return "0" + this;

	return this.toString();
};