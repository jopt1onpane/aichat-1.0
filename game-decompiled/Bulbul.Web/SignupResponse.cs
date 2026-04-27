using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct SignupResponse : IWebApiResponse
{
	public readonly int UserID;

	public readonly string DeviceID;

	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public SignupResponse(int userID, string deviceID, ErrorCode errorCode)
	{
		UserID = userID;
		DeviceID = deviceID;
		ErrorCode = errorCode;
	}
}
