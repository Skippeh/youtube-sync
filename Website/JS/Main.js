var client;
var failTimeout;
var WindowFocused = true;

function main()
{
	ShowOverlay("Initializing and connecting to master server...");
	failTimeout = setTimeout(function() {
		ShowOverlay("Failed to connect to master server within time limit.");
		setTimeout(function() {
			HideOverlay();
			client.Disconnect();
		}, 3000);
	}, 5000);

	$(document).tooltip({
		show: { effect: "fade", duration: 100 },
		hide: { effect: "fade", duration: 100 }
	});

	$("#videoUrl").watermark("Add video: http://www.youtube.com/watch?v=KvWhXOR5POY");

	//$("#leftSidebar").slideUp(0); // Hide playlist by default

	$("#chatToggleButton").click(ToggleChatVisibility);
	$("#playlistToggleButton").click(TogglePlaylistVisibility);
	
	$("#videoUrl").mouseover(FadeInVideoURL);
	$("#videoUrl").mouseleave(FadeOutVideoURL);
	$("#videoUrl").focus(FadeInVideoURL);
	$("#videoUrl").blur(FadeOutVideoURL);
	$("#videoUrl").keydown(FadeInVideoURL);

	$("#addVideoButton").click(function() {
		SubmitVideo();
		$("#videoUrl").focus();
	});

	$("#videoUrl").keydown(function(event) {
		if (event.keyCode && event.keyCode == "13") // Enter key
		{
			SubmitVideo();
			return false;
		}

		return true;
	});
	
	$("#chatInputBox").keydown(function (event) {
		if (event.keyCode && event.keyCode == "13") // Enter key pressed
		{
			SubmitChat();
			return false;
		}

		if (event.keyCode && event.keyCode == "38") // Up arrow
		{
			$("#chatInputBox").val(ChatGetHistory("up"));
			return false;
		}
		else if (event.keyCode && event.keyCode == "40") // Down arrow
		{
			$("#chatInputBox").val(ChatGetHistory("down"));
			return false;
		}

		return true;
	});

	$("#chatInputSendButton").click(function() {
		SubmitChat();
	});

	$(window).resize(ViewportResized);

	TogglePlaylistVisibility(); // Need to do this after setting BgImageTransition for buttons otherwise opacitys might be wrong.

	CreateDialogs();

	client = new Client(); // Client connects in the YoutubeApi.js file, after the youtube player has been initialized.

	TextFormatting.Initialize();

	PlaylistClearCurrentInfo();

	// Set up the youtube player
	var ytParams = { allowScriptAccess: "always" };
	var ytAtts = { id: "ytPlayer" };
	swfobject.embedSWF("http://www.youtube.com/v/QQQQQQQQQQQ?enablejsapi=1&playerapiid=ytPlayer&version=3&autohide=1&fs=0", "ytPlayerDiv", "100%", "100%", "8", null, null, ytParams, ytAtts);

	$(window).focus(function() {
		WindowFocused = true;
	});

	$(window).blur(function() {
		WindowFocused = false;
	});

	ViewportResized(); // don't judge me
}

function ToggleChatVisibility()
{
	var chatDiv = $("#rightSidebar");
	var playerDiv = $("#playerDiv");
	//var leftSidebar = $("#leftSidebar");

	var leftOpacity = $("#leftSidebar").css("opacity");
	
	var chatToggleButton = $("#chatToggleButton");
	if (chatToggleButton.css("opacity") != "1")
	{
		chatToggleButton.stop().animate(
			{
				opacity: "1"
			}, 250);

		playerDiv.stop().animate(
			{
				width: $(window).width() - (leftOpacity == "1" ? 614 : 312)
			}, 250);

		//leftSidebar.stop().animate(
		//	{
		//		width: $(window).width() - 322
		//	}, 250);

		chatDiv.stop().fadeIn(250);
	}
	else
	{
		chatToggleButton.stop().animate(
			{
				opacity: "0.2"
			}, 250);

		playerDiv.stop().animate(
			{
				width: $(window).width() - (leftOpacity == "1" ? 302 : 0)
			}, 250);

		//leftSidebar.stop().animate(
		//	{
		//		width: $(window).width() - 11
		//	}, 250);

		chatDiv.stop().fadeOut(250);
	}

	// Scroll the chat to the bottom because the scroll position is reset when the div is hidden.
	ChatScrollToBottom();
}

function TogglePlaylistVisibility()
{
	//var playlistDiv = $("#leftSidebar");
	var playerDiv = $("#playerDiv");

	var rightOpacity = $("#rightSidebar").css("display") == "none" ? 0 : 1;

	var playlistToggleButton = $("#playlistToggleButton");
	if (playlistToggleButton.css("opacity") != "1") // Fade in
	{
		playlistToggleButton.stop().animate(
			{
				opacity: "1"
			}, 250);

		playerDiv.stop().animate(
			{
				left: 302,
				width: $(document).width() - (rightOpacity == "1" ? 614 : 302)
			}, 250);

		$("#leftSidebar").stop().animate(
			{
				opacity: 1
			}, 250);
	}
	else
	{
		playlistToggleButton.stop().animate(
			{
				opacity: "0.2"
			}, 250);

		playerDiv.stop().animate(
			{
				left: 0,
				width: $(document).width() - (rightOpacity == "1" ? 312 : 0)
			}, 250);

			$("#leftSidebar").stop().animate(
				{
					opacity: 0
				}, 250);
	}
}

function FadeInVideoURL()
{
	$("#videoUrl").stop().animate(
		{
			"opacity": '1'
		}, 100);
}

function FadeOutVideoURL()
{
	if ($("#videoUrl").val() != "")
		return;
	
	$("#videoUrl").stop().animate(
		{
			"opacity": '0.1'
		}, 100);
}

function ShowOverlay(text, color)
{
	if (color == undefined) color = "#FFF";

	var overlay = $("#overlay");
	overlay.html("<span style=\"color:" + color + ";\">" +text + "</span>");

	overlay.css({ "line-height": $(window).height() + "px" });
	
	YTPause(); // Pause the player when overlay is opened.

	$("#overlay").stop().fadeIn("slow");
}

function HideOverlay()
{
	$("#overlay").fadeOut("slow");
}

function SubmitChat()
{
	var text = $("#chatInputBox").val();
	ChatClearInput();

	text = $.trim(text);

	if (text == "")
		return;

	ChatAddHistory(text);
	ChatResetHistoryPos();

	window.client.Send(
		{
			intent: "chat",
			message: text
		});
}

function SubmitVideo()
{
	var url = $("#videoUrl").val();
	$("#videoUrl").val("");
	
	if (url == "")
		return;
	
	if (!YTCheckURL(url))
	{
		SetVideoURLError("Invalid video ID.");
		return;
	}
	var startTime = parseInt(queryString("t", url));
	
	if (isNaN(startTime))
		startTime = 0;

	window.client.Send(
		{
			intent: "addVideo",
			id: queryString("v", url),
			t: startTime
		});
}

function ViewportResized()
{
	$("#overlay").css({ "line-height": $(window).height() + "px" });

	var playerDiv = $("#playerDiv");

	if ($("#chatToggleButton").css("opacity") != "1")
	{
		playerDiv.stop().animate(
			{
				width: $(window).width()
			}, 0);
	}
	else
	{
		playerDiv.stop().animate(
			{
				width: $(window).width() - 312
			}, 0);
	}
}

function SetError(text)
{
	$("#errorText").html(text);
}

function queryString(name, url)
{
	if (url == undefined)
		url = window.location.search;

	name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
	var regexS = "[\\?&#]" + name + "=([^&#]*)";
	var regex = new RegExp(regexS);
	var results = regex.exec(url);
	if (results == null)
		return "";
	else
	{
		var result = decodeURIComponent(results[1].replace(/\+/g, " "));

		// not sure if good idea
		result = result.replace(/</g, "&lt;");
		result = result.replace(/>/g, "&gt");

		return result;
	}
}

function CreateDialogs()
{
	// ----------------------------------------------- About
	$("#aboutDialog").dialog(
		{
			autoOpen: false,
			title: "About"
		});

	$("#aboutText").click(function () {
		$("#aboutDialog").dialog("open");
		$(".ui-dialog :button").blur(); // Unfocus from the close button. (jquery pls stahp)
	});

	// ----------------------------------------------- Settings
	$("#settingsDialog").dialog(
		{
			autoOpen: false,
			title: "Settings",
			width: 500,
			height: 400,
			modal: true,
			resizable: false
		});

	$("#settingsButton").click(function() {
		$("#settingsDialog").dialog("open");
	});

	$("#settingsSaveButton").click(SaveSettings);

	$("#settingsNameBox").val(CookiesGet("name", "Unnamed"));
	$("#settingsDefaultRoomBox").val(CookiesGet("room", ""));
	$("#settingsMaxDesync").val(CookiesGet("MaxDesync", "3"));
	$("#settingsPreferredQuality").val(YTGetQuality());
	$("#settingsPlayChatSounds").prop("checked", CookiesGet("ChatSounds", "1") == "1");
	$("#settingsShowChatNotifications").prop("checked", CookiesGet("ChatNotifications", "1") == "1");
	
	// ----------------------------------------------- Rooms
	$("#roomsDialog").dialog(
		{
			autoOpen: false,
			title: "Public rooms",
			width: 600,
			height: 500,
			modal: true
		});

	$("#roomsDialogButton").click(function() {
		$("#roomsDialog").dialog("open");
		$(".ui-dialog :button").blur();

		RoomListUpdate();
	});
	
	// ----------------------------------------------- Room settings
	$("#roomSettingsDialog").dialog(
		{
			autoOpen: false,
			title: "Room settings",
			width: 500,
			height: 400,
			modal: false
		});

	$("#roomSettingsButton").click(function() {
		if (client.Owner)
			$("#roomSettingsDialog").dialog("open");
	});
}

function SaveSettings()
{
	CookiesSet("name", $("#settingsNameBox").val());
	CookiesSet("room", $("#settingsDefaultRoomBox").val());
	CookiesSet("quality", $("#settingsPreferredQuality").val());
	CookiesSet("MaxDesync", $("#settingsMaxDesync").val());
	CookiesSet("ChatSounds", $("#settingsPlayChatSounds").prop("checked") == true ? "1" : "0");
	CookiesSet("ChatNotifications", $("#settingsShowChatNotifications").prop("checked") == true ? "1" : "0");
	
	if (webkitNotifications.checkPermission() == 1 && CookiesGet("ChatNotifications") == "1")
	{
		webkitNotifications.requestPermission();
		console.log("Requesting webkit notification permission.");
	}

	if (client.ClientName != $("#settingsNameBox").val())
		client.Send({ intent: "chat", message: "/name " + $("#settingsNameBox").val() });

	$("#settingsDialog").dialog("close");
}

var timeoutVideoUrl = null;
function SetVideoURLError(text)
{
	var element = $("#videoUrlError");

	clearTimeout(timeoutVideoUrl);

	element.text(text);
	element.fadeIn(100, function () {
		timeoutVideoUrl = setTimeout(function() {
			element.fadeOut(100);
		}, 5000);
	});
}