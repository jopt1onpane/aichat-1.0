using System.IO;
using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct FileDownLoadResponse : IWebApiResponse
{
	public readonly FileInfo FileInfo;

	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public FileDownLoadResponse(FileInfo fileInfo, ErrorCode errorCode)
	{
		FileInfo = fileInfo;
		ErrorCode = errorCode;
	}
}
