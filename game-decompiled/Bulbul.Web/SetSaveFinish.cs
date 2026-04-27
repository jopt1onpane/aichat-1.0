namespace Bulbul.Web;

public readonly struct SetSaveFinish(string deviceID, string sessionID, SaveDataFlash flash, bool isCheckLogin, UserStatus userStatus) : IRequest<ErrorCodeResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;

	[WebApiQueryParam("s")]
	public readonly string SessionID = sessionID;

	[WebApiQueryParam("fl")]
	public readonly SaveDataFlash Flash = flash;

	[WebApiQueryParam("icl")]
	public readonly bool IsCheckLogin = isCheckLogin;

	[WebApiForm("userStatus")]
	public readonly UserStatus UserStatus = userStatus;
}
