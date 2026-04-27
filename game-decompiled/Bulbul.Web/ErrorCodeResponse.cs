using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct ErrorCodeResponse : IWebApiResponse
{
	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public ErrorCodeResponse(ErrorCode errorCode)
	{
		ErrorCode = errorCode;
	}
}
