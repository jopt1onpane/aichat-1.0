using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct SyncSaveDataResponse : IWebApiResponse
{
	public readonly string SessionID;

	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public SyncSaveDataResponse(string sessionID, ErrorCode errorCode)
	{
		SessionID = sessionID;
		ErrorCode = errorCode;
	}
}
