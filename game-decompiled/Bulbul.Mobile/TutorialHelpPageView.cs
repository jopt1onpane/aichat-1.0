using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class TutorialHelpPageView : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	[Header("合計ページ数")]
	private int _pageSum;

	[SerializeField]
	private SerializedDictionaryEnum<GameLanguageType, Sprite[]> _localizedPageSprites;

	[SerializeField]
	[Header("")]
	private CanvasGroup _inLocalizedPageImageCanvasGroup;

	[SerializeField]
	[Header("")]
	private CanvasGroup _outLocalizedPageImageCanvasGroup;

	[SerializeField]
	private string[] _localizeHelpTextIDs;

	[SerializeField]
	private TextLocalizationBehaviour _textLocalizationBehaviour;

	[SerializeField]
	[Header("ページ数を表すドット")]
	private CanvasGroup[] _pageDots;

	[SerializeField]
	private Button _nextButton;

	[SerializeField]
	private Button _backButton;

	[SerializeField]
	[Header("最終ページのボタン")]
	private Button _lastButton;

	[SerializeField]
	[Header("右からスライドインスタート地点")]
	private float _fromPageRightX;

	[SerializeField]
	[Header("左からスライドインスタート地点")]
	private float _fromPageLeftX;

	[SerializeField]
	[Header("スライドインするゴール位置")]
	private float _toPageX;

	private int _curIdx;

	private int _prevIdx;

	private RectTransform _inRectTransform;

	private RectTransform _outRectTransform;

	private Image _inImage;

	private Image _outImage;

	private Sprite[] _usingPageSprites;

	private Subject<Unit> _onClickLastButton = new Subject<Unit>();

	private Subject<Unit> _onClickNextButton = new Subject<Unit>();

	private Subject<Unit> _onClickBackButton = new Subject<Unit>();

	private Sequence _dotSequence;

	private Sequence _imageSequence;

	public Observable<Unit> OnClickLastButton => _onClickLastButton;

	public Observable<Unit> OnClickNextButton => _onClickNextButton;

	public Observable<Unit> OnClickBackButton => _onClickBackButton;

	public void Setup()
	{
		ObservableSubscribeExtensions.Subscribe(_nextButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayClick();
			_onClickNextButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_backButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayClick();
			_onClickBackButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_lastButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayClick();
			_onClickLastButton.OnNext(Unit.Default);
		}).AddTo(this);
		_inRectTransform = _inLocalizedPageImageCanvasGroup.transform as RectTransform;
		_outRectTransform = _outLocalizedPageImageCanvasGroup.transform as RectTransform;
		_inImage = _inLocalizedPageImageCanvasGroup.GetComponent<Image>();
		_outImage = _outLocalizedPageImageCanvasGroup.GetComponent<Image>();
	}

	public void Prepare(int pageIdx)
	{
		_dotSequence?.Complete();
		_imageSequence?.Complete();
		_dotSequence = null;
		_imageSequence = null;
		_curIdx = pageIdx;
		_prevIdx = pageIdx;
		CanvasGroup[] pageDots = _pageDots;
		for (int i = 0; i < pageDots.Length; i++)
		{
			pageDots[i].alpha = 0f;
		}
		_pageDots[_curIdx].alpha = 1f;
		ReactiveProperty<GameLanguageType> gameLanguage = SaveDataManager.Instance.SettingData.GameLanguage;
		if (!_localizedPageSprites.TryGetValue(gameLanguage.CurrentValue, out _usingPageSprites))
		{
			_localizedPageSprites.TryGetValue(GameLanguageType.Japanese, out _usingPageSprites);
		}
		Vector2 anchoredPosition = _inRectTransform.anchoredPosition;
		anchoredPosition.x = _toPageX;
		_inRectTransform.anchoredPosition = anchoredPosition;
		_inLocalizedPageImageCanvasGroup.alpha = 1f;
		anchoredPosition = _outRectTransform.anchoredPosition;
		anchoredPosition.x = _fromPageRightX;
		_outRectTransform.anchoredPosition = anchoredPosition;
		_outLocalizedPageImageCanvasGroup.alpha = 0f;
		_inImage.sprite = _usingPageSprites[_curIdx];
		_textLocalizationBehaviour.Set(_localizeHelpTextIDs[_curIdx]);
		UpdateButtons();
	}

	public void Move(int idx)
	{
		_prevIdx = _curIdx;
		_curIdx = idx;
		bool isNext = idx > _prevIdx;
		UpdateButtons();
		UpdateDots();
		UpdateImage(isNext);
		UpdateText();
	}

	private void UpdateButtons()
	{
		int num = _pageSum - 1;
		_backButton.gameObject.SetActive(_curIdx != 0);
		_nextButton.gameObject.SetActive(_curIdx < num);
		_lastButton.gameObject.SetActive(_curIdx == num);
	}

	private void UpdateDots()
	{
		_dotSequence?.Complete();
		_dotSequence = DOTween.Sequence();
		_dotSequence.Join(_pageDots[_prevIdx].DOFade(0f, 0.2f));
		_dotSequence.Join(_pageDots[_curIdx].DOFade(1f, 0.2f));
	}

	private void UpdateImage(bool isNext)
	{
		_imageSequence?.Complete();
		_imageSequence = DOTween.Sequence();
		Vector2 anchoredPosition = _inRectTransform.anchoredPosition;
		anchoredPosition.x = (isNext ? _fromPageRightX : _fromPageLeftX);
		_inRectTransform.anchoredPosition = anchoredPosition;
		_inImage.sprite = _usingPageSprites[_curIdx];
		anchoredPosition = _outRectTransform.anchoredPosition;
		anchoredPosition.x = _toPageX;
		_outRectTransform.anchoredPosition = anchoredPosition;
		_outImage.sprite = _usingPageSprites[_prevIdx];
		_inLocalizedPageImageCanvasGroup.alpha = 0f;
		_outLocalizedPageImageCanvasGroup.alpha = 1f;
		_imageSequence.Join(_outLocalizedPageImageCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutQuint));
		_imageSequence.Join(_inLocalizedPageImageCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.InQuart));
		_imageSequence.Join(_inRectTransform.DOAnchorPosX(_toPageX, 0.2f).SetEase(Ease.InQuart));
	}

	private void UpdateText()
	{
		_textLocalizationBehaviour.Set(_localizeHelpTextIDs[_curIdx]);
	}

	private void OnDisable()
	{
		_dotSequence?.Complete();
		_imageSequence?.Complete();
		_dotSequence = null;
		_imageSequence = null;
	}
}
