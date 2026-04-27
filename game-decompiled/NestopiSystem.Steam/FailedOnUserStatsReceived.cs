using Steamworks;

namespace NestopiSystem.Steam;

public class FailedOnUserStatsReceived : SteamException
{
	public EResult Result { get; }

	public FailedOnUserStatsReceived(string message, EResult result)
		: base(message)
	{
		Result = result;
	}
}
