using System;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class DecorationCategoryTabListViewForMobile : MonoBehaviour
{
	private static readonly string _unreleasedLocalizationID = "ui_lock_title";

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	[Header("DecorationCategoryの順番に入れてください")]
	private InteractableUISettingTab[] _tabs;

	private TextLocalizationBehaviour[] _textlocalizations;

	private string[] _defaultLocalzationIDs;

	private Button[] _tabButtons;

	private bool[] _tabStates;

	private Subject<(DecorationService.DecorationCategoryType, bool)> _onChangedTab = new Subject<(DecorationService.DecorationCategoryType, bool)>();

	private DecorationService.DecorationCategoryType _currentType;

	private InteractableUISettingTab _currentTab;

	public Observable<(DecorationService.DecorationCategoryType, bool)> OnChangedTab => _onChangedTab;

	private void OnDestroy()
	{
		_onChangedTab.Dispose();
	}

	private void Start()
	{
		InteractableUISettingTab[] tabs = _tabs;
		for (int i = 0; i < tabs.Length; i++)
		{
			tabs[i].DeactivateUseUI(isUseDoComplete: true);
		}
		if (!(_currentTab == null))
		{
			_currentTab.ActivateUseUI(isUseDoComplete: true);
		}
	}

	public void Setup()
	{
		int num = 0;
		_textlocalizations = new TextLocalizationBehaviour[_tabs.Length];
		_tabButtons = new Button[_tabs.Length];
		_defaultLocalzationIDs = new string[_tabs.Length];
		_tabStates = new bool[_tabs.Length];
		foreach (DecorationService.DecorationCategoryType value in Enum.GetValues(typeof(DecorationService.DecorationCategoryType)))
		{
			DecorationService.DecorationCategoryType t = value;
			InteractableUISettingTab interactableUISettingTab = _tabs[num];
			Button component = interactableUISettingTab.GetComponent<Button>();
			_tabButtons[num] = component;
			int idx = num;
			ObservableSubscribeExtensions.Subscribe(component.OnClickAsObservable(), delegate
			{
				_systemSeService.PlaySelect();
				OnClickTab(t, idx);
			}).AddTo(this);
			_textlocalizations[num] = interactableUISettingTab.GetComponentInChildren<TextLocalizationBehaviour>(includeInactive: true);
			_defaultLocalzationIDs[num] = (string.IsNullOrEmpty(_textlocalizations[num].LocalizeID) ? _textlocalizations[num].Text.text : _textlocalizations[num].LocalizeID);
			num++;
		}
	}

	public void SetTabUnReleased(DecorationService.DecorationCategoryType type)
	{
		int num = (int)(type - 1);
		_textlocalizations[num].Set(_unreleasedLocalizationID);
		_tabStates[num] = false;
	}

	public void SetTabDefault(DecorationService.DecorationCategoryType type)
	{
		int num = (int)(type - 1);
		_textlocalizations[num].Set(_defaultLocalzationIDs[num]);
		_tabStates[num] = true;
	}

	private void OnClickTab(DecorationService.DecorationCategoryType type, int idx)
	{
		if (_currentType == type)
		{
			return;
		}
		InteractableUISettingTab interactableUISettingTab = _tabs[idx];
		_currentTab?.DeactivateUseUI();
		interactableUISettingTab.ActivateUseUI();
		_currentType = type;
		_currentTab = interactableUISettingTab;
		RectTransform rectTransform = interactableUISettingTab.RectTransform;
		RectTransform content = _scrollRect.content;
		RectTransform viewport = _scrollRect.viewport;
		float posX;
		float num = (posX = GetPosX(rectTransform)) + rectTransform.rect.width;
		float width = viewport.rect.width;
		float width2 = content.rect.width;
		if (width2 > width)
		{
			float num3;
			float num2 = (num3 = _scrollRect.horizontalNormalizedPosition * (width2 - width)) + width;
			if (posX < num3)
			{
				_scrollRect.ScrollToHorizontal(rectTransform, 0f);
			}
			else if (num > num2)
			{
				_scrollRect.ScrollToHorizontal(rectTransform, 1f);
			}
		}
		_onChangedTab.OnNext((_currentType, _tabStates[idx]));
	}

	private float GetPosX(RectTransform transform)
	{
		return transform.localPosition.x + transform.rect.x;
	}

	public void SetTab(DecorationService.DecorationCategoryType type)
	{
		InteractableUISettingTab[] tabs = _tabs;
		for (int i = 0; i < tabs.Length; i++)
		{
			tabs[i].DeactivateUseUI(isUseDoComplete: true);
		}
		int num = (int)(type - 1);
		_currentTab = _tabs[num];
		_currentType = type;
		_currentTab.ActivateUseUI(isUseDoComplete: true);
		_onChangedTab.OnNext((_currentType, _tabStates[num]));
	}

	public bool GetIsUnlockTab(DecorationService.DecorationCategoryType type)
	{
		int num = (int)(type - 1);
		return _tabStates[num];
	}
}
