namespace Bulbul.Web;

public readonly struct Purchase(string deviceID, string receipt) : IRequest<PurchaseResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;

	[WebApiForm("receipt")]
	public readonly string Receipt = receipt;
}
