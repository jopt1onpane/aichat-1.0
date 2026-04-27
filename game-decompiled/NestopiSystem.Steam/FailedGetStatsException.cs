namespace NestopiSystem.Steam;

public class FailedGetStatsException : SteamException
{
	public FailedGetStatsException()
	{
	}

	public FailedGetStatsException(string message)
		: base(message)
	{
	}
}
