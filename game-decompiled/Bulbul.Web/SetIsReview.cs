namespace Bulbul.Web;

public readonly struct SetIsReview(string deviceID) : IRequest<Nil>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;
}
