namespace Bulbul;

public enum ErrorCode
{
	None = 0,
	ApiHeader = 101,
	SaveDataUploadFailed = 201,
	SaveDataNotFound = 202,
	DeviceIdInvalid = 300,
	DeviceLogout = 301,
	InsertDeviceFailed = 310,
	UpdateDeviceUserFailed = 311,
	DeviceLoginFailed = 320,
	AccountInvalid = 401,
	InsertAccountFailed = 410,
	UpdateAccountUserFailed = 411,
	ReleaseLinkageFailed = 420,
	LinkSessionExpired = 430
}
