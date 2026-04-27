namespace NestopiSystem.Steam;

public class SteamAPIFailedInitializeException : SteamException
{
	public SteamAPIFailedInitializeException()
	{
	}

	public SteamAPIFailedInitializeException(string message)
		: base(message)
	{
	}
}
