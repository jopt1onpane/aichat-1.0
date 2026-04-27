using System;

namespace Bulbul.Web;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class WebApiRequestAttribute : Attribute
{
	public string Route;

	public WebApiRequestAttribute(string route)
	{
		Route = route;
	}
}
