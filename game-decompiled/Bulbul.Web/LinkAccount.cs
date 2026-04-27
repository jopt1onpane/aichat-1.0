namespace Bulbul.Web;

public readonly struct LinkAccount(string deviceID, string linkSessionID, AccountType accountType, bool isTransfer) : IRequest<ErrorCodeResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;

	[WebApiQueryParam("ls")]
	public readonly string LinkSessionID = linkSessionID;

	[WebApiQueryParam("at")]
	public readonly AccountType AccountType = accountType;

	[WebApiQueryParam("it")]
	public readonly bool IsTransfer = isTransfer;
}
