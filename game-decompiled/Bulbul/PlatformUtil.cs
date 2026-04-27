using System;

namespace Bulbul;

[Obsolete("DevicePlatformを使ってください")]
public static class PlatformUtil
{
	public enum PlatformType
	{
		PC,
		Mobile
	}

	public static PlatformType GetCurrentPlatform()
	{
		return PlatformType.PC;
	}

	public static bool IsPC()
	{
		return GetCurrentPlatform() == PlatformType.PC;
	}

	public static bool IsMobile()
	{
		return GetCurrentPlatform() == PlatformType.Mobile;
	}
}
