Playlist = {};

Playlist.Add = function(title, author, length, videoId, uniqueId, channelImage)
{
	var playlistEntries = $("#playlistEntries");

	var fullTitle = title;
	if (title.length > 45)
		title = title.substring(0, 42) + "...";

	playlistEntries.append(
		"<div id=\"playlistEntry_" + uniqueId + "\" class=\"playlistEntry\">" +
			"<a target=\"_blank\" href=\"http://www.youtube.com/watch?feature=player_embedded&v=" + videoId + "\"><img class=\"channelImage\" src=\"" + channelImage + "\" width=\"21\" height=\"21\" title=\"Goto video page\"/></a>" +
			"<span class=\"videoTitle\" title=\"" + fullTitle + "\">" + title + "</span><br/>" +
			"<span class=\"videoDetails\">Uploaded by " + author + " " + length + "</span>" +
		"</div>"
	);
	
	$(".scrollbar").TrackpadScrollEmulator("recalculate");
};

Playlist.Remove = function(uniqueId)
{
	$("#playlistEntry_" + uniqueId).remove();
	$(".scrollbar").TrackpadScrollEmulator("recalculate");
};

Playlist.Clear = function()
{
	$("#playlistEntries").html("");
	$(".scrollbar").TrackpadScrollEmulator("recalculate");
};

Playlist.SetCurrentInfo = function(title, videoImage, description)
{
	$("#currentVideo").stop().animate({ opacity: "1" }, 500);

	$("#currentVideoTitle").html(title);
	$("#currentVideoDescription").html(description);
	$("#currentVideoImage").attr("src", videoImage);
};

Playlist.ClearCurrentInfo = function()
{
	$("#currentVideo").stop().animate({ opacity: "0.5" }, 500);

	$("#currentVideoTitle").html("No video playing");
	$("#currentVideoDescription").html("");
	$("#currentVideoImage").attr("src", "Images/noVideoImage.png");
};