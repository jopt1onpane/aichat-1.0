namespace Bulbul.Web;

public readonly struct CheckLogin(string deviceID, ulong readNewsID, GameLanguageType language) : IRequest<CheckLoginResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;

	[WebApiQueryParam("n")]
	public readonly ulong ReadNewsID = readNewsID;

	[WebApiQueryParam("l")]
	public readonly GameLanguageType Language = language;
}
