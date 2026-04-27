using System;

namespace NestopiSystem.Steam;

public class SteamException : Exception
{
	public SteamException()
	{
	}

	public SteamException(string message)
		: base(message)
	{
	}
}
