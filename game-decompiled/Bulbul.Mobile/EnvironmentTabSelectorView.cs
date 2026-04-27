using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class EnvironmentTabSelectorView : MonoBehaviour
{
	[SerializeField]
	private EnvironmentTabView _viewAndSoundTab;

	[SerializeField]
	private EnvironmentTabView _viewOnlyTab;

	[SerializeField]
	private EnvironmentTabView _soundOnlyTab;

	private readonly Subject<EnvironmentControllerType> _onClickTab = new Subject<EnvironmentControllerType>();

	public Observable<EnvironmentControllerType> OnClickTab => _onClickTab;

	public EnvironmentControllerType CurrentTab { get; private set; }

	public void Initialize()
	{
		ObservableSubscribeExtensions.Subscribe(_viewAndSoundTab.OnClick, delegate
		{
			_onClickTab.OnNext(EnvironmentControllerType.ViewAndSound);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_viewOnlyTab.OnClick, delegate
		{
			_onClickTab.OnNext(EnvironmentControllerType.ViewOnly);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_soundOnlyTab.OnClick, delegate
		{
			_onClickTab.OnNext(EnvironmentControllerType.SoundOnly);
		}).AddTo(this);
	}

	public void SelectTab(EnvironmentControllerType controllerType)
	{
		CurrentTab = controllerType;
		_viewAndSoundTab.SetSelected(controllerType == EnvironmentControllerType.ViewAndSound);
		_viewOnlyTab.SetSelected(controllerType == EnvironmentControllerType.ViewOnly);
		_soundOnlyTab.SetSelected(controllerType == EnvironmentControllerType.SoundOnly);
	}
}
