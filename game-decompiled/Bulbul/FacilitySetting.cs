using UnityEngine;
using VContainer;

namespace Bulbul;

public class FacilitySetting : MonoBehaviour
{
	[Inject]
	private SettingService _settingService;

	[Inject]
	private ISettingUI _settingUI;

	public void Setup()
	{
		_settingUI.Setup();
		Deactivate();
	}

	public bool IsActive()
	{
		if (_settingUI.IsActive())
		{
			return true;
		}
		return false;
	}

	public void Activate()
	{
		_settingService.SelectSettingType(SettingType.General);
		_settingUI.Activate();
	}

	public void Deactivate()
	{
		_settingUI.Deactivate();
	}
}
