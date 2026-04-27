using System;
using UnityEngine;

namespace Bulbul.Mobile;

public class MobilePlatformRoot : MonoBehaviour, IPlatformRoot
{
	[SerializeField]
	private UIManagerForMobile _uiManager;

	public void Setup(Action tutorialSkip)
	{
		_uiManager.Setup(tutorialSkip);
	}

	public void UpdatePlatform()
	{
		_uiManager.UpdatePlatform();
	}

	public void UpdateMusicOnly()
	{
		_uiManager.UpdateMusicOnly();
	}
}
