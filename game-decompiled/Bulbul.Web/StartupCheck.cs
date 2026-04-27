using UnityEngine;

namespace Bulbul.Web;

public readonly struct StartupCheck : IRequest<StartupCheckResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID;

	[WebApiQueryParam("l")]
	public readonly GameLanguageType Language;

	[WebApiQueryParam("p")]
	public DevicePlatform PlatformType => DevicePlatform.Steam;

	[WebApiQueryParam("v")]
	public string AppVersion => Application.version;

	public StartupCheck(string deviceID, GameLanguageType language)
	{
		DeviceID = deviceID;
		Language = language;
	}
}
