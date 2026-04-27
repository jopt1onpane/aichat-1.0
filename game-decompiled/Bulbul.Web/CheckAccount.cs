namespace Bulbul.Web;

public readonly struct CheckAccount(string deviceID, string token, AccountType accountType) : IRequest<CheckAccountResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;

	[WebApiForm("token")]
	public readonly string Token = token;

	[WebApiQueryParam("at")]
	public readonly AccountType AccountType = accountType;
}
