namespace Bulbul.Web;

public readonly struct DeleteAccount(string deviceID) : IRequest<ErrorCodeResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;
}
