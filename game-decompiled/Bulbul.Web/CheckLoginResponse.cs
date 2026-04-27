using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct CheckLoginResponse : IWebApiResponse
{
	public readonly bool IsLogin;

	public readonly bool IsNewsBadge;

	public readonly ShopMaintenance ShopMaintenanceInfo;

	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public CheckLoginResponse(bool isLogin, bool isNewsBadge, ShopMaintenance shopMaintenanceInfo, ErrorCode errorCode)
	{
		IsLogin = isLogin;
		IsNewsBadge = isNewsBadge;
		ShopMaintenanceInfo = shopMaintenanceInfo;
		ErrorCode = errorCode;
	}
}
