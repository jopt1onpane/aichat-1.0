namespace Bulbul.Web;

public readonly struct GetShopSettings(string deviceID) : IRequest<GetShopSettingsResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;
}
