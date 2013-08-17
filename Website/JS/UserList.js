UserList = {};

UserList.AllUsers = {};

UserList.Add = function(id, name)
{
	UserList.AllUsers[id] = name;
};

UserList.Remove = function(id)
{
	delete UserList.AllUsers[id];
};

UserList.Clear = function()
{
	UserList.AllUsers = {};
};