namespace Bulbul.Web;

public readonly struct GetSaveData : IFileRequest<FileDownLoadResponse>, IRequest<FileDownLoadResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID;

	[WebApiQueryParam("f")]
	public readonly string FileName;

	string IFileRequest<FileDownLoadResponse>.FileName => FileName;

	public GetSaveData(string deviceID, string fileName)
	{
		DeviceID = deviceID;
		FileName = fileName;
	}
}
