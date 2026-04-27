namespace Bulbul.Web;

[WebApiRequest("releaseAccount")]
public readonly struct UnlinkAccount(string deviceID, AccountType accountType) : IRequest<ErrorCodeResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;

	[WebApiQueryParam("at")]
	public readonly AccountType AccountType = accountType;
}
