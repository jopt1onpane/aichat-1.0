using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class MainSceneButtonsForMobile : MonoBehaviour
{
	[SerializeField]
	private UIManagerForMobile _uIManagerForMobile;

	[SerializeField]
	private ExitUI _exitUI;

	[SerializeField]
	private Button _exitButton;

	[SerializeField]
	private Button _noteButton;

	[SerializeField]
	private Button _calendarButton;

	[SerializeField]
	private Button _todoButton;

	[SerializeField]
	private Button _uiHideButton;

	[SerializeField]
	private Button _decorationButton;

	[SerializeField]
	private Button _environmentButton;

	[SerializeField]
	private Button[] _musicListButtons;

	[SerializeField]
	private Button _storyButton;

	[SerializeField]
	private Button _specialButton;

	[SerializeField]
	private Button _settingButton;

	[SerializeField]
	private Button _habitButton;

	[SerializeField]
	private Button _shopButton;

	private void Start()
	{
		Setup();
	}

	public void Setup()
	{
		_exitUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_exitButton.OnClickAsObservable(), delegate
		{
			_exitUI.OnClickButtonExitGameIcon();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_exitUI.OnExitGame, delegate
		{
			_uIManagerForMobile.OnClickExitButton();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_noteButton.OnClickAsObservable(), delegate
		{
			_uIManagerForMobile.OnClickButtonFacilityNote();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_calendarButton.OnClickAsObservable(), delegate
		{
			_uIManagerForMobile.OnClickButtonFacilityCalendar();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_todoButton.OnClickAsObservable(), delegate
		{
			_uIManagerForMobile.OnClickButtonFacilityTodo();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_uiHideButton.OnClickAsObservable(), delegate
		{
			_uIManagerForMobile.OnClickButtonAllUIDeactivate();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_decorationButton.OnClickAsObservable(), delegate
		{
			_uIManagerForMobile.OnClickButtonFacilityDecoration();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_environmentButton.OnClickAsObservable(), delegate
		{
			_uIManagerForMobile.OnClickButtonFacilityEnvironment();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe((from _ in _musicListButtons
			where _ != null
			select _.OnClickAsObservable()).Merge(), delegate
		{
			_uIManagerForMobile.OnClickButtonFacilityMusic();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_storyButton.OnClickAsObservable(), delegate
		{
			_uIManagerForMobile.OnClickButtonFacilityStory();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_specialButton.OnClickAsObservable(), delegate
		{
			_uIManagerForMobile.OnClickButtonFacilitySpecial();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_settingButton.OnClickAsObservable(), delegate
		{
			_uIManagerForMobile.OnClickButtonFacilitySetting();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_habitButton.OnClickAsObservable(), delegate
		{
			_uIManagerForMobile.OnClickButtonFacilityHabitTracker();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_shopButton.OnClickAsObservable(), delegate
		{
			_uIManagerForMobile.OnClickButtonFacilityShop();
		}).AddTo(this);
	}
}
