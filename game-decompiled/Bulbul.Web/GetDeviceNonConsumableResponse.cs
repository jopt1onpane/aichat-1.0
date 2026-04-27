using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct GetDeviceNonConsumableResponse : IWebApiResponse
{
	public readonly UnlockProducts UnlockProducts;

	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public GetDeviceNonConsumableResponse(ErrorCode errorCode, UnlockProducts unlockProducts)
	{
		ErrorCode = errorCode;
		UnlockProducts = unlockProducts;
	}
}
