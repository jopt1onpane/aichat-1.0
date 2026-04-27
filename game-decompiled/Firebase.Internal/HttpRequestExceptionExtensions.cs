using System.Net;
using System.Net.Http;

namespace Firebase.Internal;

internal static class HttpRequestExceptionExtensions
{
	internal static HttpStatusCode? GetStatusCode(this HttpRequestException exception)
	{
		if (exception.Data.Contains("StatusCode"))
		{
			return (HttpStatusCode)exception.Data["StatusCode"];
		}
		return null;
	}
}
