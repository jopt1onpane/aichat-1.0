namespace Bulbul.Web;

public readonly struct CheckPurchase(string deviceID, string productID, GameLanguageType language) : IRequest<GetCheckPurchaseResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;

	[WebApiQueryParam("p")]
	public readonly string ProductID = productID;

	[WebApiQueryParam("l")]
	public readonly GameLanguageType Language = language;
}
