using System;
using DG.Tweening;
using NestopiSystem;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class SelectCalendarDayUI : MonoBehaviour
{
	[Inject]
	private DateService _dateService;

	[SerializeField]
	[Header("インタラクトUI")]
	private InteractableUI _interactableUI;

	[SerializeField]
	[Header("日付表示時のBaseImage")]
	private Image _dayShowBaseImage;

	[SerializeField]
	[Header("作業チェックImage")]
	private Image _workCheckImage;

	[SerializeField]
	[Header("日付テキスト")]
	private TextMeshProUGUI _dayText;

	[SerializeField]
	[Header("選択ボタン")]
	private Button _selectButton;

	[SerializeField]
	private Color dayTextColor;

	[SerializeField]
	private Color selectDayTextColor;

	[SerializeField]
	private Sprite backSprite;

	[SerializeField]
	private Sprite mouseoverBackSprite;

	[SerializeField]
	private Sprite todayBackSprite;

	[SerializeField]
	private Sprite mouseoverTodayBackSprite;

	private readonly Subject<SelectCalendarDayUI> onSelected = new Subject<SelectCalendarDayUI>();

	private bool isMouseover;

	private bool backImageDirty;

	private bool isSelected;

	private bool resetOnDisable;

	public string DayText => _dayText.text;

	public Observable<SelectCalendarDayUI> OnSelected => onSelected;

	public DateTime DateTime { get; private set; }

	public int Day => DateTime.Day;

	private void Start()
	{
		if (_dateService == null)
		{
			_dateService = RoomLifetimeScope.Resolve<DateService>();
		}
		if (!DevicePlatform.Steam.IsMobile())
		{
			Observable.Merge<bool>(from _ in _interactableUI.OnPointerEnterAsObservable()
				select true, from _ in _interactableUI.OnPointerExitAsObservable()
				select false).Subscribe(delegate(bool mouseover)
			{
				isMouseover = mouseover;
				backImageDirty = true;
			}).AddTo(this);
		}
		ObservableSubscribeExtensions.Subscribe(_dateService.OnChangeDate, delegate
		{
			backImageDirty = true;
		}).AddTo(this);
	}

	public void Setup(bool isValidWorkCheck, bool resetOnDisable)
	{
		_workCheckImage.gameObject.SetActive(isValidWorkCheck);
		this.resetOnDisable = resetOnDisable;
	}

	public void OnClickButtonSelectDay()
	{
		onSelected.OnNext(this);
	}

	public void OnSelectDay()
	{
		_dayText.color = selectDayTextColor;
		isSelected = true;
		backImageDirty = true;
	}

	public void OnSelectOtherButton(bool isUseDoComplete = false)
	{
		_dayText.color = dayTextColor;
		isSelected = false;
		backImageDirty = true;
	}

	public void Activate(DateTime dateTime, bool isWork)
	{
		DateTime = dateTime;
		_selectButton.enabled = true;
		_dayText.text = Day.ToString();
		_dayShowBaseImage.DOFade(1f, 0f).Complete();
		UpdateWorkedUI(isWork);
		backImageDirty = true;
	}

	public void Deactivate(bool isUseDoComplete = false)
	{
		_interactableUI.DeactivateUseUI();
		_selectButton.enabled = false;
		_dayText.text = string.Empty;
		_dayShowBaseImage.DOFade(0f, 0f).Complete();
		_workCheckImage.DOFade(0f, 0f).Complete();
	}

	private void BackSpriteUpdate()
	{
		if (isMouseover || isSelected)
		{
			Sprite sprite = (DateTime.IsSameDay(DateTime.Now) ? mouseoverTodayBackSprite : mouseoverBackSprite);
			_dayShowBaseImage.sprite = sprite;
		}
		else
		{
			Sprite sprite2 = (DateTime.IsSameDay(DateTime.Now) ? todayBackSprite : backSprite);
			_dayShowBaseImage.sprite = sprite2;
		}
	}

	public void UpdateWorkedUI(bool isWork)
	{
		if (isWork)
		{
			_workCheckImage.SetAlpha(1f);
		}
		else
		{
			_workCheckImage.SetAlpha(0f);
		}
	}

	private void LateUpdate()
	{
		if (backImageDirty)
		{
			backImageDirty = false;
			BackSpriteUpdate();
		}
	}

	private void OnDisable()
	{
		if (resetOnDisable)
		{
			isMouseover = false;
			isSelected = false;
			_dayShowBaseImage.sprite = (DateTime.IsSameDay(DateTime.Now) ? todayBackSprite : backSprite);
			_dayText.color = dayTextColor;
		}
	}
}
