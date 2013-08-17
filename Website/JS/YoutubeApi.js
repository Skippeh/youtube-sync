YT = {};

YT.Player = null;

YT.Seek = function(seconds)
{
	console.log("YTSeek(" + seconds + ")");
	if (YT.Player)
		YT.Player.seekTo(seconds, true); // Allow seeking to unbuffered areas
};

YT.Play = function()
{
	if (YT.Player)
		YT.Player.playVideo();
};

YT.Pause = function()
{
	if (YT.Player)
		YT.Player.pauseVideo();
};

YT.Stop = function()
{
	if (YT.Player)
		YT.Player.stopVideo();
};

YT.SetVideo = function(videoId, elapsed)
{
	if (!elapsed)
		elapsed = 0;

	if (YT.Player)
		YT.Player.loadVideoById(videoId, elapsed, YT.GetQuality());
};

YT.SetState = function(state)
{
	if (!YT.Player)
		return;
	
	if (state == 0) // Stop
		YT.Stop();
	else if (state == 1) // Play
		YT.Play();
	else if (state == 2) // Pause
		YT.Pause();
	else if (state == 3) // Buffering
		YT.Pause();
};

YT.GetState = function()
{
	if (!YT.Player)
		return -1;

	return YT.Player.getPlayerState();
};

YT.SetQuality = function(quality)
{
	if (YT.Player)
		YT.Player.setPlaybackQuality(quality);
};

YT.GetQuality = function()
{
	return Cookies.Get("quality", "hd720");
};

// Returns true if the url is a valid youtube link.
YT.CheckURL = function(url)
{
	return /((http|https):\/\/)?(www\.)?(youtube\.com)(\/)?([a-zA-Z0-9\-\.]+)\/?/.test(url) && queryString("v", url) != "";
};

YT.GetElapsed = function()
{
	if (YT.Player)
		return YT.Player.getCurrentTime();

	return 0;
};

// Youtube api events
function onYouTubePlayerReady(playerId) {
	YT.Player = document.getElementById("YT.Player");
	YT.Player.addEventListener("onStateChange", "onYoutubePlayerStateChanged");
	console.log("Youtube player ready");

	Client.Connect();
}

function onYoutubePlayerStateChanged(newState) {
	/*
		-1 (unstarted)
		0 (ended)
		1 (playing)
		2 (paused)
		3 (buffering)
		5 (video cued)
	*/

	if (Client.IsPrivileged()) {
		console.log("NewState/ClientState: " + newState + " - " + Client.CurrentVideoState);
		if (newState != -1) {
			Client.SendNewState(newState);
		}
	}

	if (newState == 0) {
		Playlist.ClearCurrentInfo();
	}
}