var AllUsers = { };

function UserListAdd(id, name)
{
	AllUsers[id] = name;
}

function UserListRemove(id)
{
	delete AllUsers[id];
}

function UserListClear()
{
	AllUsers = { };
}