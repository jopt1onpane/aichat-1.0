using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class WindowBehavior : MonoBehaviour
{
	[Inject]
	private WindowViewService _windowViewService;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	private WindowViewType _windowViewType;

	private Subject<WindowViewType> _onDeactivateWindow = new Subject<WindowViewType>();

	private Subject<WindowViewType> _onActivateWindow = new Subject<WindowViewType>();

	public WindowViewType WindowViewType => _windowViewType;

	public Observable<WindowViewType> OnDeactivateWindow => _onDeactivateWindow;

	public Observable<WindowViewType> OnActivateWindow => _onActivateWindow;

	public void Setup(WindowViewType windowViewType)
	{
		_windowViewType = windowViewType;
		ApplyWindowBySaveData();
	}

	public void ApplyWindowBySaveData()
	{
		if (_environmentDataService.IsWindowActive(_windowViewType))
		{
			if (!SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.CurrentValue || (_windowViewType != WindowViewType.Day && _windowViewType != WindowViewType.Sunset && _windowViewType != WindowViewType.Night && _windowViewType != WindowViewType.Cloudy))
			{
				ActivateWindowView();
			}
		}
		else
		{
			DeactivateWindowView();
		}
	}

	public void ChangeWindowView(ChangeType changeType)
	{
		switch (changeType)
		{
		case ChangeType.Activate:
			ActivateWindowView();
			break;
		case ChangeType.Deactivate:
			DeactivateWindowView();
			break;
		case ChangeType.Switch:
			if (_windowViewService.IsActiveWindow(_windowViewType))
			{
				DeactivateWindowView();
			}
			else
			{
				ActivateWindowView();
			}
			break;
		}
	}

	private void ActivateWindowView()
	{
		_windowViewService.ActivateWindow(_windowViewType);
		_onActivateWindow.OnNext(_windowViewType);
		_environmentDataService.SetViewActive(_windowViewType, isActive: true);
	}

	private void DeactivateWindowView()
	{
		_windowViewService.DeactivateWindow(_windowViewType);
		_onDeactivateWindow.OnNext(_windowViewType);
		_environmentDataService.SetViewActive(_windowViewType, isActive: false);
	}

	public bool IsActiveWindow()
	{
		return _environmentDataService.IsWindowActive(_windowViewType);
	}
}
