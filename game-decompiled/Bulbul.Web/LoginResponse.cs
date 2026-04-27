using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct LoginResponse : IWebApiResponse
{
	public readonly int UserID;

	public readonly List<AccountType> LinkedAccounts;

	public readonly UnlockProducts UnlockProducts;

	public readonly bool IsReview;

	public readonly ShopMaintenance ShopMaintenanceInfo;

	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public LoginResponse(int userID, List<AccountType> linkedAccounts, UnlockProducts unlockProducts, bool isReview, ShopMaintenance shopMaintenanceInfo, ErrorCode errorCode)
	{
		UserID = userID;
		LinkedAccounts = linkedAccounts;
		UnlockProducts = unlockProducts;
		IsReview = isReview;
		ShopMaintenanceInfo = shopMaintenanceInfo;
		ErrorCode = errorCode;
	}
}
