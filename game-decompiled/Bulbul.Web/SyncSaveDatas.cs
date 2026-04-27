using System.IO;

namespace Bulbul.Web;

[WebApiRequest("syncSaveData")]
public readonly struct SyncSaveDatas(string deviceID, string sessionID, SaveDataFlash flash, bool isCheckLogin, UserStatus userStatus, FileInfo[] fileInfo = null, string[] deletedNames = null) : IRequest<SyncSaveDataResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;

	[WebApiQueryParam("s")]
	public readonly string SessionID = sessionID;

	[WebApiForm("files[]")]
	public readonly FileInfo[] FileInfo = fileInfo;

	[WebApiForm("deleted[]")]
	public readonly string[] DeletedNames = deletedNames;

	[WebApiQueryParam("fl")]
	public readonly SaveDataFlash Flash = flash;

	[WebApiQueryParam("icl")]
	public readonly bool IsCheckLogin = isCheckLogin;

	[WebApiForm("userStatus")]
	public readonly UserStatus UserStatus = userStatus;
}
