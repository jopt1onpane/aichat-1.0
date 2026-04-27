namespace Bulbul;

public static class DevicePlatformExtension
{
	public static bool IsPC(this DevicePlatform platform)
	{
		if (platform != DevicePlatform.Steam && platform != DevicePlatform.Editor)
		{
			return platform == DevicePlatform.Switch;
		}
		return true;
	}

	public static bool IsMobile(this DevicePlatform platform)
	{
		if (platform != DevicePlatform.Android)
		{
			return platform == DevicePlatform.iOS;
		}
		return true;
	}

	public static bool IsConsole(this DevicePlatform platform)
	{
		return platform == DevicePlatform.Switch;
	}
}
