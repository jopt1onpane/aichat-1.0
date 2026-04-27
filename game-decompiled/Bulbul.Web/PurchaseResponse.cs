using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct PurchaseResponse : IWebApiResponse
{
	public ErrorCode ErrorCode { get; }

	public bool IsGranted { get; }

	public string TransactionId { get; }

	[JsonConstructor]
	public PurchaseResponse(ErrorCode errorCode, bool isGranted, string transactionId)
	{
		ErrorCode = errorCode;
		IsGranted = isGranted;
		TransactionId = transactionId;
	}
}
