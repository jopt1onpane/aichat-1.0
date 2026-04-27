namespace Bulbul.Web;

public readonly struct GetDeviceNonConsumable(string deviceID) : IRequest<GetDeviceNonConsumableResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;
}
