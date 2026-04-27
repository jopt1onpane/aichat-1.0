namespace Bulbul.Web;

public readonly struct Login(string deviceID, GameLanguageType language) : IRequest<LoginResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;

	[WebApiQueryParam("l")]
	public readonly GameLanguageType Language = language;
}
