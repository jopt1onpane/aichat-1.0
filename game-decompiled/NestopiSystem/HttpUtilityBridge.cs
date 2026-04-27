using System.Collections.Specialized;
using System.Web;

namespace NestopiSystem;

public static class HttpUtilityBridge
{
	public static NameValueCollection ParseQueryString(string query)
	{
		return HttpUtility.ParseQueryString(query);
	}
}
