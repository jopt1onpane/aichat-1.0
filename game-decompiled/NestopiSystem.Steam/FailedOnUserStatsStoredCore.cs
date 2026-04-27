using Steamworks;

namespace NestopiSystem.Steam;

public class FailedOnUserStatsStoredCore : SteamException
{
	public EResult Result { get; }

	public FailedOnUserStatsStoredCore(string message, EResult result)
		: base(message)
	{
		Result = result;
	}
}
