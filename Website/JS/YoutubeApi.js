var ytPlayer = null;

function YTSeek(seconds)
{
	console.log("YTSeek(" + seconds + ")");
	if (ytPlayer)
		ytPlayer.seekTo(seconds, true); // Allow seeking to unbuffered areas
}

function YTPlay()
{
	if (ytPlayer)
		ytPlayer.playVideo();
}

function YTPause()
{
	if (ytPlayer)
		ytPlayer.pauseVideo();
}

function YTStop()
{
	if (ytPlayer)
		ytPlayer.stopVideo();
}

function YTSetVideo(videoId, elapsed)
{
	if (!elapsed)
		elapsed = 0;

	if (ytPlayer)
		ytPlayer.loadVideoById(videoId, elapsed, YTGetQuality());
}

function YTSetState(state)
{
	if (!ytPlayer)
		return;
	
	if (state == 0) // Stop
		YTStop();
	else if (state == 1) // Play
		YTPlay();
	else if (state == 2) // Pause
		YTPause();
	else if (state == 3) // Buffering
		YTPause();
}

function YTGetState()
{
	if (!ytPlayer)
		return -1;

	return ytPlayer.getPlayerState();
}

function YTSetQuality(quality)
{
	if (ytPlayer)
		ytPlayer.setPlaybackQuality(quality);
}

function YTGetQuality()
{
	return CookiesGet("quality", "hd720");
}

// Youtube api events
function onYouTubePlayerReady(playerId)
{
	ytPlayer = document.getElementById("ytPlayer");
	ytPlayer.addEventListener("onStateChange", "onYoutubePlayerStateChanged");
	console.log("Youtube player ready");

	client.Connect();
}

function onYoutubePlayerStateChanged(newState)
{
	/*
		-1 (unstarted)
		0 (ended)
		1 (playing)
		2 (paused)
		3 (buffering)
		5 (video cued)
	*/

	if (client.IsPrivileged())
	{
		console.log("NewState/ClientState: " + newState + " - " + client.CurrentVideoState);
		if (newState != -1)
		{
			client.SendNewState(newState);
		}
	}
	
	if (newState == 0)
	{
		PlaylistClearCurrentInfo();
	}
}

// Returns true if the url is a valid youtube link.
function YTCheckURL(url)
{
	return /((http|https):\/\/)?(www\.)?(youtube\.com)(\/)?([a-zA-Z0-9\-\.]+)\/?/.test(url) && queryString("v", url) != "";
}

function YTGetElapsed()
{
	if (ytPlayer)
		return ytPlayer.getCurrentTime();

	return 0;
}