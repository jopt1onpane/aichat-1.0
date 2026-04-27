using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class MainSceneButtonsForPC : MonoBehaviour
{
	[SerializeField]
	private UIManagerForPC _uIManagerForPC;

	[SerializeField]
	private ExitUI _exitUI;

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
	private Button _musicListButton;

	[SerializeField]
	private Button _storyButton;

	[SerializeField]
	private Button _specialButton;

	[SerializeField]
	private Button _settingButton;

	[SerializeField]
	private Button _habitButton;

	private void Start()
	{
		Setup();
	}

	public void Setup()
	{
		_exitUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_exitUI.OnExitGame, delegate
		{
			_uIManagerForPC.OnClickExitButton();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_noteButton.OnClickAsObservable(), delegate
		{
			_uIManagerForPC.OnClickButtonFacilityNote();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_calendarButton.OnClickAsObservable(), delegate
		{
			_uIManagerForPC.OnClickButtonFacilityCalendar();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_todoButton.OnClickAsObservable(), delegate
		{
			_uIManagerForPC.OnClickButtonFacilityTodo();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_uiHideButton.OnClickAsObservable(), delegate
		{
			_uIManagerForPC.OnClickButtonAllUIDeactivate();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_decorationButton.OnClickAsObservable(), delegate
		{
			_uIManagerForPC.OnClickButtonFacilityDecoration();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_environmentButton.OnClickAsObservable(), delegate
		{
			_uIManagerForPC.OnClickButtonFacilityEnviroment();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_musicListButton.OnClickAsObservable(), delegate
		{
			_uIManagerForPC.OnClickButtonFacilityMusic();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_storyButton.OnClickAsObservable(), delegate
		{
			_uIManagerForPC.OnClickButtonFacilityStory();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_specialButton.OnClickAsObservable(), delegate
		{
			_uIManagerForPC.OnClickButtonFacilitySpecial();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_settingButton.OnClickAsObservable(), delegate
		{
			_uIManagerForPC.OnClickButtonFacilitySetting();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_habitButton.OnClickAsObservable(), delegate
		{
			_uIManagerForPC.OnClickButtonFacilityHabitTracker();
		}).AddTo(this);
	}
}
