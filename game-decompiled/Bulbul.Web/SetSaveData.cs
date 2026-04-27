using System.IO;

namespace Bulbul.Web;

public readonly struct SetSaveData(string deviceID, bool isCheckLogin, UserStatus userStatus, FileInfo[] uploadFileInfos = null, string[] deleteFileNames = null) : IRequest<ErrorCodeResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;

	[WebApiQueryParam("icl")]
	public readonly bool IsCheckLogin = isCheckLogin;

	[WebApiForm("files[]")]
	public readonly FileInfo[] UploadFileInfos = uploadFileInfos;

	[WebApiForm("deleted[]")]
	public readonly string[] DeleteFileNames = deleteFileNames;

	[WebApiForm("userStatus")]
	public readonly UserStatus UserStatus = userStatus;
}
