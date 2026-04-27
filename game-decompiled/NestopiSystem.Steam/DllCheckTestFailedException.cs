namespace NestopiSystem.Steam;

public class DllCheckTestFailedException : SteamException
{
	public DllCheckTestFailedException()
	{
	}

	public DllCheckTestFailedException(string message)
		: base(message)
	{
	}
}
