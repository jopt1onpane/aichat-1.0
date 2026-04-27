using UnityEngine;

namespace NestopiSystem;

public static class Applicationx
{
	public static void OpenURL(string url)
	{
		Application.OpenURL(url);
	}

	public static void Quit()
	{
		Application.Quit();
	}

	public static string GetDeviceName()
	{
		return SystemInfo.deviceModel;
	}
}
