namespace Bulbul.Web;

public readonly struct UpdateTermsVer(string deviceID) : IRequest<ErrorCodeResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;
}
