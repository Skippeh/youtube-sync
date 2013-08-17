TextFormatting = {};

TextFormatting.keyValues = [];

TextFormatting.FormatText = function(text)
{
	for (var key in this.keyValues)
	{
		var val = this.keyValues[key];

		// img = case insensitive, multiline, match all. (not in that order)
		text = text.replace(new RegExp("&lt;" + key + "&gt;", "img"), "<span style=\"" + val + "\">");
		text = text.replace(new RegExp("&lt;/" + key + "&gt;", "img"), "</span>");
	}

	// Linkify URL's.
	text = this.Linkify(text);
	
	// Turn \n into <br/>.
	text = text.replace(/\n/g, "<br/>");

	return text;
};

TextFormatting.AddTag = function(key, val)
{
	this.keyValues[key] = val;
};

TextFormatting.Initialize = function()
{
	this.AddTag("b", "font-weight:bold;");					// <B>		- Bold
	this.AddTag("u", "text-decoration:underline;");			// <U>		- Underline
};

TextFormatting.Linkify = function (text)
{
	var replacedText, replacePattern1, replacePattern2, replacePattern3;

	//URLs starting with http://, https://, or ftp://
	replacePattern1 = /(\b(https?|ftp):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/gim;
	replacedText = text.replace(replacePattern1, '<a href="$1" target="_blank">$1</a>');

	//URLs starting with "www." (without // before it, or it'd re-link the ones done above).
	replacePattern2 = /(^|[^\/])(www\.[\S]+(\b|$))/gim;
	replacedText = replacedText.replace(replacePattern2, '$1<a href="http://$2" target="_blank">$2</a>');

	//Change email addresses to mailto:: links.
	replacePattern3 = /(\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,6})/gim;
	replacedText = replacedText.replace(replacePattern3, '<a href="mailto:$1">$1</a>');

	return replacedText;
};