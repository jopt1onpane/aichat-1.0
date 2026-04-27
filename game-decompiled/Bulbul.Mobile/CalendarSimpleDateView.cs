using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class CalendarSimpleDateView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _yearNumText;

	[SerializeField]
	private TextMeshProUGUI _monthNumText;

	[SerializeField]
	private TextMeshProUGUI _dayNumText;

	[SerializeField]
	private Button _backButton;

	[SerializeField]
	private Button _nextButton;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	private RectTransform __rectTransform;

	private Sequence sequence;

	private static readonly float _durationSec = 0.25f;

	private RectTransform _rectTransform
	{
		get
		{
			if (__rectTransform == null)
			{
				__rectTransform = base.transform as RectTransform;
			}
			return __rectTransform;
		}
	}

	public Observable<Unit> OnClickbackButton => _backButton.OnClickAsObservable();

	public Observable<Unit> OnClickNextButton => _nextButton.OnClickAsObservable();

	public void SetDateText(int year, int month, int day)
	{
		_yearNumText.SetText("{0}", year);
		_monthNumText.SetText("{0:00}", month);
		_dayNumText.SetText("{0:00}", day);
	}

	public void Activate()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		_canvasGroup.blocksRaycasts = false;
		base.gameObject.SetActive(value: true);
		sequence.Join(_canvasGroup.DOFade(1f, _durationSec).SetEase(Ease.InQuint));
		sequence.Join(_rectTransform.DOAnchorPosY(0f, _durationSec).SetEase(Ease.InCubic));
		sequence.OnComplete(delegate
		{
			_canvasGroup.blocksRaycasts = true;
		});
	}

	public void Deactivate()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		_canvasGroup.blocksRaycasts = false;
		base.gameObject.SetActive(value: true);
		sequence.Join(_canvasGroup.DOFade(0f, _durationSec).SetEase(Ease.InCubic));
		sequence.Join(_rectTransform.DOAnchorPosY(5f, _durationSec).SetEase(Ease.InCubic));
		sequence.OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	public void DeactivateImmidiate()
	{
		Deactivate();
		sequence.Complete();
		sequence = null;
	}
}
