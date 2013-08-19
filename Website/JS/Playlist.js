Playlist = {};

Playlist.Add = function(title, author, length, videoId, uniqueId, channelImage)
{
	var playlistEntries = $("#playlistEntries");

	var fullTitle = title;
	if (title.length > 45)
		title = title.substring(0, 42) + "...";

	playlistEntries.append(
		"<div id=\"playlistEntry_" + uniqueId + "\" class=\"playlistEntry\" style=\"background-color: rgba(51, 119, 219, 0.5);\">" +
			"<a target=\"_blank\" href=\"http://www.youtube.com/watch?feature=player_embedded&v=" + videoId + "\"><img class=\"channelImage\" src=\"" + channelImage + "\" width=\"21\" height=\"21\" title=\"Goto video page\"/></a>" +
			"<span class=\"videoTitle\" title=\"" + fullTitle + "\">" + title + "</span><br/>" +
			"<span class=\"videoDetails\">Uploaded by " + author + " " + length + "</span>" +
		"</div>"
	);
	
	var elm = $("#playlistEntry_" + uniqueId);
	elm.animate({ "background-color": "rgba(0,0,0,0)" }, 350);
	
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

Playlist.SetCurrentInfo = function(title, videoId, description)
{
	$("#currentVideo").stop().animate({ opacity: "1" }, animspd(500));

	var link = "http://www.youtube.com/watch?v=" + videoId;
	$("#currentVideoTitle").html("<a style='text-decoration: none;' href='" + link + "' target='_blank'>" + title + "</a>");
	$("#currentVideoDescription").html(description);
	$("#currentVideoImage").attr("src", "http://i2.ytimg.com/vi/" + videoId + "/hqdefault.jpg");
	$("#currentVideoImageLink").attr("href", link);
};

Playlist.ClearCurrentInfo = function()
{
	$("#currentVideo").stop().animate({ opacity: "0.5" }, animspd(500));

	$("#currentVideoTitle").html("No video playing");
	$("#currentVideoDescription").html("");
	$("#currentVideoImage").attr("src", "Images/noVideoImage.png");
};