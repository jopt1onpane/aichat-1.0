namespace NestopiSystem.Steam;

public class RestartAppIfNecessaryFromAPIException : SteamException
{
	public RestartAppIfNecessaryFromAPIException()
	{
	}

	public RestartAppIfNecessaryFromAPIException(string message)
		: base(message)
	{
	}
}
