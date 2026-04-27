using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct GetShopSettingsResponse : IWebApiResponse
{
	public readonly ShopSetting[] ShopSettings;

	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public GetShopSettingsResponse(ShopSetting[] shopSettings, ErrorCode errorCode)
	{
		ShopSettings = shopSettings;
		ErrorCode = errorCode;
	}
}
