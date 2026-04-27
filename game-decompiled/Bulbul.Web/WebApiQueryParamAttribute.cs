using System;

namespace Bulbul.Web;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class WebApiQueryParamAttribute : Attribute
{
	public string Key;

	public WebApiQueryParamAttribute(string key)
	{
		Key = key;
	}
}
