function RoomListClear()
{
	$("#roomsTable tr:not(.tableHeader)").remove();
}

function RoomListAdd(name, count)
{
	var attr = "";
	
	if (client.RoomName == name)
		attr = "class=\"currentRoom\"";

	$("#roomsTable").append("<tr " + attr + " onclick=\"RoomListOnClick('" + name + "');\"><td>" + name + "</td><td>" + count + "</td></tr>");
}

function RoomListOnClick(name)
{
	if (client.RoomName == null || (client.RoomName != name && confirm("Are you sure you want to leave the current room?")))
	{
		client.Send(
			{
				intent: "chat",
				message: "/room " + name
			});

		$("#roomsDialog").dialog("close");
	}
}

function RoomListUpdate()
{
	RoomListClear();
	RoomListAdd("Getting rooms...", "");
	client.Send({ intent: "getPublicRooms" });
}