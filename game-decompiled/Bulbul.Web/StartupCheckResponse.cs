using System.Text.Json.Serialization;
using NestopiSystem;

namespace Bulbul.Web;

public readonly struct StartupCheckResponse : IWebApiResponse
{
	public readonly bool IsMaintenance;

	public readonly string MaintenanceStart;

	public readonly string MaintenanceEnd;

	public readonly string MainText;

	public readonly bool IsUpdateApp;

	public readonly bool IsConsentRequired;

	public readonly bool IsDeleteUser;

	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public StartupCheckResponse(bool isMaintenance, string maintenanceStart, string maintenanceEnd, string mainText, bool isUpdateApp, bool isConsentRequired, bool isDeleteUser, ErrorCode errorCode)
	{
		IsMaintenance = isMaintenance;
		MaintenanceStart = maintenanceStart;
		MaintenanceEnd = maintenanceEnd;
		MainText = mainText;
		IsUpdateApp = isUpdateApp;
		IsConsentRequired = isConsentRequired;
		IsDeleteUser = isDeleteUser;
		ErrorCode = errorCode;
	}

	public void ThrowResetException(ResetReason ignoreReason = ResetReason.None)
	{
		ThrowIf(IsUpdateApp, ignoreReason, ResetReason.AppForceUpdate);
		ThrowIf(IsMaintenance, ignoreReason, ResetReason.Maintenance);
		ThrowIf(IsConsentRequired, ignoreReason, ResetReason.TermsUpdate);
		static void ThrowIf(bool flag, ResetReason self, ResetReason ignore)
		{
			if (flag && !self.HasAnyFlag(ignore))
			{
				throw new AppResetException(ignore);
			}
		}
	}
}
