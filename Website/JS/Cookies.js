Cookies = {};

Cookies.Set = function(name, value)
{
	name = encodeURI(name);
	value = encodeURI(value);

	document.cookie = name + "=" + value.replace(/=/g, "&#61;")
										.replace(/</g, "&lt;")
										.replace(/>/g, "&gt");
};

// Code taken from http://www.w3schools.com/js/js_cookies.asp
Cookies.Get = function(name, defaultValue)
{
	if (undef(defaultValue))
		defaultValue = "";
	name = encodeURI(name);
	
	var cValue = document.cookie;
	var cStart = cValue.indexOf(" " + name + "=");
	if (cStart == -1) {
		cStart = cValue.indexOf(name + "=");
	}
	if (cStart == -1) {
		cValue = defaultValue;
		Cookies.Set(name, encodeURI(defaultValue));
	}
	else {
		cStart = cValue.indexOf("=", cStart) + 1;
		var c_end = cValue.indexOf(";", cStart);
		if (c_end == -1) {
			c_end = cValue.length;
		}
		cValue = unescape(cValue.substring(cStart, c_end));
	}

	if (decodeURI(cValue) == "")
	{
		Cookies.Set(name, encodeURI(defaultValue));
		return defaultValue;
	}

	return decodeURI(cValue);
};