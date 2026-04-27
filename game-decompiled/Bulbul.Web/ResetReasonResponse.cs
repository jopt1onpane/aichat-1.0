using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct ResetReasonResponse : IWebApiResponse
{
	[JsonPropertyName("reset_reason")]
	public readonly ResetReason ResetReason;

	[JsonIgnore]
	ErrorCode IWebApiResponse.ErrorCode => ErrorCode.None;

	[JsonConstructor]
	public ResetReasonResponse(ResetReason resetReason)
	{
		ResetReason = resetReason;
	}
}
