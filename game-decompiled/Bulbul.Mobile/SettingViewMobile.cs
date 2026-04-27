using UnityEngine;

namespace Bulbul.Mobile;

public class SettingViewMobile : MonoBehaviour, ISettingUI
{
	public void Setup()
	{
	}

	public bool IsActive()
	{
		return base.gameObject.activeSelf;
	}

	public void Activate()
	{
	}

	public void Deactivate()
	{
	}
}
