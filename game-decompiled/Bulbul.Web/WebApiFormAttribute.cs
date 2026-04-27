using System;

namespace Bulbul.Web;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class WebApiFormAttribute : Attribute
{
	public string Key;

	public WebApiFormAttribute(string key)
	{
		Key = key;
	}
}
