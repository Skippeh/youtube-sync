<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Youtube Sync</title>
	<link href="style.css" rel="stylesheet" />
	<link href="gradient.css" rel="stylesheet" />
	<link rel="stylesheet" href="http://code.jquery.com/ui/1.10.3/themes/smoothness/jquery-ui.css" />
	<!--[if gte IE 9]>
	  <style type="text/css">
		.gradient {
		   filter: none;
		}
	  </style>
	<![endif]-->
	
	<script src="//ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min.js"></script>
	<script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.10.3/jquery-ui.min.js"></script>
	<script src="JS/jquery.watermark.min.js"></script>
	<script src="http://ajax.googleapis.com/ajax/libs/swfobject/2.2/swfobject.js"></script>
	<script src="JS/Cookies.js"></script>
	<script src="JS/Client.js"></script>
	<script src="JS/YoutubeApi.js"></script>
	<script src="JS/Chat.js"></script>
	<script src="JS/BgImageTransition.js"></script>
	<script src="JS/Main.js"></script>
	<script>
		$(document).ready(function() {
			main();
		});
	</script>
</head>
<body>
	<div id="overlay"></div>

	<div id="topBar">
		<span class="rightAligned" id="settingsButton" title="Open settings"></span>
		<span class="rightAligned" id="chatToggleButton" title="Toggle chat"></span>
		<span class="leftAligned" id="playlistToggleButton" title="Toggle playlist"></span>
	</div>
	
	<div id="leftSidebar">
		<!-- Playlist -->
	</div>
	
	<div id="rightSidebar">
		<div id="chat" class="chat">
			<span id="chatText"></span>
		</div>
		<div id="chatInput">
			<input type="text" id="chatInputBox"/>
			<span id="chatInputSendButton" style="cursor:pointer;">Send</span>
		</div>
	</div>

	<div id="wrapper">
		<div id="playerDiv">
			<div id="ytPlayerDiv">You need flash player 8+ and javascript enabled to view and use this site.</div>
		</div>
	</div>
	
	<div id="controlBar">
		<input type="text" class="controls" name="videoUrl" id="videoUrl" />
		<span id="addVideoButton">Add</span>
		<span id="videoUrlError"></span>

		<span class="footer" style="cursor:pointer;" id="aboutText">Youtube Sync v1</span>
		<span class="footer" id="errorText"></span>
	</div>
	
	<!-- Dialogs -->
	
	<div id="aboutDialog" class="dialogWindow">
		<h2>Credits</h2>
		<span>Created by Jonathan Lindahl. c:</span>
	</div>
	
	<div id="settingsDialog" class="dialogWindow">
		<table>
			<tr>
				<td>Name</td>
				<td><input type="text" id="settingsNameBox"/></td>
			</tr>
			<tr>
				<td>Default room</td>
				<td><input type="text" id="settingsDefaultRoomBox"/></td>
			</tr>
			<tr>
				<td>Preferred quality</td>
				<td>
					<select id="settingsPreferredQuality">
						<option value="default">Default</option>
					    <option value="hd1080">1080p</option>
						<option value="hd720">720p</option>
						<option value="large">480p</option>
						<option value="medium">360p</option>
						<option value="small">240p</option>
				    </select>
				</td>
			</tr>
		</table>
		
		<span id="settingsSaveButton">Save</span>
	</div>

	<!-- End of dialogs -->
</body>
</html>