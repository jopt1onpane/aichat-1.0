namespace Bulbul.Web;

public readonly struct GetNews(string deviceID, GameLanguageType language) : IRequest<GetNewsResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;

	[WebApiQueryParam("l")]
	public readonly GameLanguageType Language = language;
}
