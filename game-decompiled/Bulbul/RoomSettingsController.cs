using UnityEngine;

namespace Bulbul;

public class RoomSettingsController
{
	public void OnInputFPS(int fps)
	{
		if ((float)fps <= 0f)
		{
			Debug.LogError("fps must be positive");
		}
		else
		{
			Application.targetFrameRate = fps;
		}
	}

	public void OnInputFullScreenMode(bool isFullScreen)
	{
		FullScreenMode fullscreenMode = ((!isFullScreen) ? FullScreenMode.Windowed : FullScreenMode.ExclusiveFullScreen);
		Screen.SetResolution(Screen.width, Screen.height, fullscreenMode);
	}
}
