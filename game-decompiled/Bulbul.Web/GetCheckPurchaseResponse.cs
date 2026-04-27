using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct GetCheckPurchaseResponse : IWebApiResponse
{
	public ErrorCode ErrorCode { get; }

	public bool IsPossible { get; }

	public ShopMaintenance ShopMaintenanceInfo { get; }

	public int Reason { get; }

	[JsonConstructor]
	public GetCheckPurchaseResponse(ErrorCode errorCode, bool isPossible, int reason, ShopMaintenance shopMaintenanceInfo)
	{
		ErrorCode = errorCode;
		IsPossible = isPossible;
		Reason = reason;
		ShopMaintenanceInfo = shopMaintenanceInfo;
	}
}
