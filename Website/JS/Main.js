var failTimeout;
var WindowFocused = true;
var AnimationsEnabled = true;

function main()
{
	ShowOverlay("Initializing and connecting to master server...");
	failTimeout = setTimeout(function() {
		ShowOverlay("Failed to connect to master server within time limit.");
		setTimeout(function() {
			HideOverlay();
			Client.Disconnect();
		}, 3000);
	}, 5000);

	$(document).tooltip({
		show: { effect: "fade", duration: 100 },
		hide: { effect: "fade", duration: 100 }
	});

	$("#videoUrl").watermark("Add video: http://www.youtube.com/watch?v=KvWhXOR5POY");

	// Simulate a cursor blinking in the "Fork me on GitHub" text.
	var forktext = $("#forktext");
	forktext.attr("href", Globals.GithubURL);
	setInterval(function () {
		var html = forktext.html();
		if (html.substr(-1) == "_") {
			forktext.html(html.substring(0, html.length - 1));
		}
		else {
			forktext.html(html + "_");
		}
	}, 530);

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
			$("#chatInputBox").val(Chat.GetHistory("up"));
			return false;
		}
		else if (event.keyCode && event.keyCode == "40") // Down arrow
		{
			$("#chatInputBox").val(Chat.GetHistory("down"));
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

	TextFormatting.Initialize();

	Playlist.ClearCurrentInfo();

	// Set up the youtube player
	var ytParams = { allowScriptAccess: "always" };
	var ytAtts = { id: "YT.Player" };
	swfobject.embedSWF("http://www.youtube.com/v/QQQQQQQQQQQ?enablejsapi=1&playerapiid=ytPlayer&version=3&autohide=1&fs=0", "ytPlayerDiv", "100%", "100%", "8", null, null, ytParams, ytAtts);

	$(window).focus(function() {
		WindowFocused = true;
	});

	$(window).blur(function() {
		WindowFocused = false;
	});
	
	$(".scrollbar").TrackpadScrollEmulator({
		
	});
	
	if (Cookies.Get("AnimationsEnabled", Globals.AnimationsEnabledDefault) == "1")
		AnimationsEnabled = true;
	else
		AnimationsEnabled = false;

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
			}, animspd(250));

		playerDiv.stop().animate(
			{
				width: $(window).width() - (leftOpacity == "1" ? 614 : 312)
			}, animspd(250));

		//leftSidebar.stop().animate(
		//	{
		//		width: $(window).width() - 322
		//	}, animspd(250));

		chatDiv.stop().fadeIn(animspd(250));
	}
	else
	{
		chatToggleButton.stop().animate(
			{
				opacity: "0.2"
			}, animspd(250));

		playerDiv.stop().animate(
			{
				width: $(window).width() - (leftOpacity == "1" ? 302 : 0)
			}, animspd(250));

		//leftSidebar.stop().animate(
		//	{
		//		width: $(window).width() - 11
		//	}, animspd(250));

		chatDiv.stop().fadeOut(animspd(250));
	}

	// Scroll the chat to the bottom because the scroll position is reset when the div is hidden.
	Chat.ScrollToBottom();
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
			}, animspd(250));

		playerDiv.stop().animate(
			{
				left: 302,
				width: $(document).width() - (rightOpacity == "1" ? 614 : 302)
			}, animspd(250));

		$("#leftSidebar").stop().animate(
			{
				opacity: 1
			}, animspd(250));
	}
	else
	{
		playlistToggleButton.stop().animate(
			{
				opacity: "0.2"
			}, animspd(250));

		playerDiv.stop().animate(
			{
				left: 0,
				width: $(document).width() - (rightOpacity == "1" ? 312 : 0)
			}, animspd(250));

			$("#leftSidebar").stop().animate(
				{
					opacity: 0
				}, animspd(250));
	}
}

function FadeInVideoURL()
{
	$("#videoUrl").stop().animate(
		{
			"opacity": '1'
		}, animspd(100));
}

function FadeOutVideoURL()
{
	if ($("#videoUrl").val() != "")
		return;
	
	$("#videoUrl").stop().animate(
		{
			"opacity": '0.1'
		}, animspd(100));
}

function ShowOverlay(text, color)
{
	if (undef(color)) color = "#FFF";

	var overlay = $("#overlay");
	overlay.html("<span style=\"color:" + color + ";\">" +text + "</span>");

	overlay.css({ "line-height": $(window).height() + "px" });

	$("#overlay").fadeIn("slow");

	YT.Pause(); // Pause the player when overlay is opened.
}

function HideOverlay()
{
	$("#overlay").fadeOut("slow");
}

function SubmitChat()
{
	var text = $("#chatInputBox").val();
	Chat.ClearInput();

	text = $.trim(text);

	if (text == "")
		return;

	Chat.AddHistory(text);
	Chat.ResetHistoryPos();

	Client.Send(
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
	
	if (!YT.CheckURL(url))
	{
		SetVideoURLError("Invalid video ID.");
		return;
	}
	var startTime = parseInt(queryString("t", url));
	
	if (isNaN(startTime))
		startTime = 0;

	Client.Send(
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
			}, animspd(0));
	}
	else
	{
		playerDiv.stop().animate(
			{
				width: $(window).width() - 312
			}, animspd(0));
	}
	
	$(".scrollbar").TrackpadScrollEmulator("recalculate");
}

function SetError(text)
{
	$("#errorText").html(text);
}

function queryString(name, url)
{
	if (undef(url))
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

	$("#settingsNameBox").val(Cookies.Get("name", "Unnamed"));
	$("#settingsDefaultRoomBox").val(Cookies.Get("room", ""));
	$("#settingsMaxDesync").val(Cookies.Get("MaxDesync", "3"));
	$("#settingsPreferredQuality").val(YT.GetQuality());
	$("#settingsPlayChatSounds").prop("checked", Cookies.Get("ChatSounds", "1") == "1");
	$("#settingsShowChatNotifications").prop("checked", Cookies.Get("ChatNotifications", "1") == "1");
	$("#settingsEnableAnimations").prop("checked", Cookies.Get("AnimationsEnabled", Globals.AnimationsEnabledDefault) == "1");
	
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

		RoomList.Update();
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
		if (Client.Owner)
			$("#roomSettingsDialog").dialog("open");
	});
}

function SaveSettings()
{
	Cookies.Set("name", $("#settingsNameBox").val());
	Cookies.Set("room", $("#settingsDefaultRoomBox").val());
	Cookies.Set("quality", $("#settingsPreferredQuality").val());
	Cookies.Set("MaxDesync", $("#settingsMaxDesync").val());
	Cookies.Set("ChatSounds", $("#settingsPlayChatSounds").prop("checked") == true ? "1" : "0");
	Cookies.Set("ChatNotifications", $("#settingsShowChatNotifications").prop("checked") == true ? "1" : "0");
	Cookies.Set("AnimationsEnabled", $("#settingsEnableAnimations").prop("checked") == true ? "1" : "0");
	AnimationsEnabled = Cookies.Get("AnimationsEnabled") == "1" ? true : false;
	
	if (!undef(window.webkitNotifications) && Cookies.Get("ChatNotifications") == "1")
	{
		if (window.webkitNotifications.checkPermission() == 1 )
		{
			window.webkitNotifications.requestPermission();
			console.log("Requesting webkit notification permission.");
		}
	}
	else if (!window.webkitNotifications && Cookies.Get("ChatNotifications") == "1")
	{
		Cookies.Set("ChatNotifications", "0");
		alert("Notifications not supported, can't enable.");
		$("#settingsShowChatNotifications").attr("checked", false);
	}

	if (Client.ClientName != $("#settingsNameBox").val())
		Client.Send({ intent: "chat", message: "/name " + $("#settingsNameBox").val() });

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

function undef(obj)
{
	if (obj == null)
		return true;
	
	return typeof (obj) === "undefined";
}

function animspd(speed)
{
	if (AnimationsEnabled)
		return speed;
	
	return 0;
}