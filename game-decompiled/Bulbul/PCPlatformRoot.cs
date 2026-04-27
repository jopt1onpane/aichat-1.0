using System;
using UnityEngine;

namespace Bulbul;

public class PCPlatformRoot : MonoBehaviour, IPlatformRoot
{
	[SerializeField]
	private UIManagerForPC _uiManagerForPC;

	public void Setup(Action tutorialSkip)
	{
		_uiManagerForPC.Setup(tutorialSkip);
	}

	public void UpdatePlatform()
	{
		_uiManagerForPC.UpdatePlatform();
	}

	public void UpdateMusicOnly()
	{
		_uiManagerForPC.UpdateMusicOnly();
	}
}
