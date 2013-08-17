RoomList = {};

RoomList.Clear = function()
{
	$("#roomsTable tr:not(.tableHeader)").remove();
};

RoomList.Add = function(name, count)
{
	var attr = "";
	
	if (Client.RoomName == name)
		attr = "class=\"currentRoom\"";

	$("#roomsTable").append("<tr " + attr + " onclick=\"RoomList.OnClick('" + name + "');\"><td>" + name + "</td><td>" + count + "</td></tr>");
};

RoomList.OnClick = function(name)
{
	if (Client.RoomName == null || (Client.RoomName != name && confirm("Are you sure you want to leave the current room?")))
	{
		Client.Send(
			{
				intent: "chat",
				message: "/room " + name
			});

		$("#roomsDialog").dialog("close");
	}
};

RoomList.Update = function()
{
	RoomList.Clear();
	RoomList.Add("Getting rooms...", "");
	Client.Send({ intent: "getPublicRooms" });
};