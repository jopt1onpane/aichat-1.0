using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using R3;
using UnityEngine;

namespace Bulbul;

public class SentenceSelectionButtonsUI : MonoBehaviour
{
	public class ButtonEventInfo
	{
		public string SelectionText;

		public Action Callback;

		public bool IsUsed;
	}

	[SerializeField]
	private SentenceSelectionButton _selectionButtonaPrefab;

	[SerializeField]
	private List<SentenceSelectionButton> _selectionButtons = new List<SentenceSelectionButton>();

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private float _fromPosY;

	private float _toPosY;

	private readonly Subject<Unit> _onSelectionButtonClicked = new Subject<Unit>();

	public ReactiveProperty<bool> IsEnabled { get; private set; } = new ReactiveProperty<bool>(value: false);

	public Observable<Unit> OnSelectionButtonClicked => _onSelectionButtonClicked;

	private void Awake()
	{
		_rectTransform = base.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
		_selectionButtons = base.transform.GetComponentsInChildren<SentenceSelectionButton>(includeInactive: true).ToList();
		DisableButtons();
	}

	private void Start()
	{
	}

	public void EnableButtons(List<ButtonEventInfo> buttonEventInfo)
	{
		CreateIfNotEnough(buttonEventInfo.Count);
		DisableButtons(isDoComplete: true);
		for (int i = 0; i < buttonEventInfo.Count; i++)
		{
			ButtonEventInfo info = buttonEventInfo[i];
			SentenceSelectionButton sentenceSelectionButton = _selectionButtons[i];
			sentenceSelectionButton.SelectionText.SetText(info.SelectionText);
			sentenceSelectionButton.Button.onClick.AddListener(delegate
			{
				info.Callback?.Invoke();
				IsEnabled.Value = false;
				_onSelectionButtonClicked.OnNext(Unit.Default);
				DisableButtons();
			});
			if (info.IsUsed)
			{
				sentenceSelectionButton.ColorToGrayOut();
			}
			else
			{
				sentenceSelectionButton.ColorToNormal();
			}
			sentenceSelectionButton.gameObject.SetActive(value: true);
		}
		base.gameObject.SetActive(value: true);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
		IsEnabled.Value = true;
	}

	public void DisableButtons(bool isDoComplete = false)
	{
		_moveTween?.Kill();
		_fadeTween?.Kill();
		foreach (SentenceSelectionButton selectionButton in _selectionButtons)
		{
			selectionButton.RemoveAllListeners();
		}
		_moveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			foreach (SentenceSelectionButton selectionButton2 in _selectionButtons)
			{
				selectionButton2.DisableButton();
			}
			base.gameObject.SetActive(value: false);
		});
		if (isDoComplete)
		{
			_moveTween.Complete();
			_fadeTween.Complete();
		}
	}

	private void CreateIfNotEnough(int needCount)
	{
		if (needCount > _selectionButtons.Count)
		{
			int num = needCount - _selectionButtons.Count;
			for (int i = 0; i < num; i++)
			{
				SentenceSelectionButton item = UnityEngine.Object.Instantiate(_selectionButtonaPrefab, base.transform);
				_selectionButtons.Add(item);
			}
		}
	}
}
