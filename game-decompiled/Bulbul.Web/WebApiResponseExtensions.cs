using NestopiSystem;

namespace Bulbul.Web;

public static class WebApiResponseExtensions
{
	public static bool WasLogout<T>(this WebApiResponse<T> self) where T : IWebApiResponse
	{
		return self.Error.WasLogout();
	}

	public static bool WasLogout(this WebApiError self)
	{
		if (self.ErrorCode != ErrorCode.DeviceLogout && !self.ResetReason.HasAnyFlag(ResetReason.OtherDeviceLogin))
		{
			return self.Exception is LogoutException;
		}
		return true;
	}
}
