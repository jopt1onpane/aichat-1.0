using R3;
using UnityEngine;

namespace Bulbul;

public class FacilityEnvironment : MonoBehaviour, IApplyEnvironmentWindowController
{
	[SerializeField]
	private EnvironmentUI _enviromentUI;

	public ReadOnlyReactiveProperty<bool> IsActive => _enviromentUI.IsActive;

	public void Setup()
	{
		_enviromentUI.Setup();
	}

	public void UpdateFacility()
	{
	}

	public void ApplyWindowBySavedata()
	{
		_enviromentUI.ApplyWindowBySavedata();
	}

	public void Activate()
	{
		_enviromentUI.Activate();
	}

	public void Deactivate()
	{
		_enviromentUI.Deactivate();
	}
}
