namespace NestopiSystem.Steam;

public class PacksizeTestFailedException : SteamException
{
	public PacksizeTestFailedException()
	{
	}

	public PacksizeTestFailedException(string message)
		: base(message)
	{
	}
}
