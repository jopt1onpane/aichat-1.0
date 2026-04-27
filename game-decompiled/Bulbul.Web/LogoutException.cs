namespace Bulbul.Web;

public class LogoutException : AppResetException
{
	public readonly ErrorCode ErrorCode;

	public LogoutException()
		: base(ResetReason.OtherDeviceLogin)
	{
		ErrorCode = ErrorCode.DeviceLogout;
	}
}
