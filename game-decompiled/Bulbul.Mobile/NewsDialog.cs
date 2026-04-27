using Bulbul.Web;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class NewsDialog : MonoBehaviour
{
	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private FacilityAnimationBase _dialogAnimation;

	[SerializeField]
	private ClickOutsideDetector _outsideDetector;

	[SerializeField]
	private TextMeshProUGUI _dateText;

	[SerializeField]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	private TextMeshProUGUI _bodyText;

	[SerializeField]
	private ScrollRect _scrollRect;

	private Subject<Unit> onClose = new Subject<Unit>();

	public Observable<Unit> OnClose => onClose;

	private void OnDestroy()
	{
		onClose?.Dispose();
	}

	public void Setup()
	{
		if ((bool)_dialogAnimation)
		{
			_dialogAnimation.Setup();
		}
		ObservableSubscribeExtensions.Subscribe(_closeButton.OnClickAsObservable(), delegate
		{
			onClose?.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_outsideDetector.OnClickOutside, delegate
		{
			onClose?.OnNext(Unit.Default);
		}).AddTo(this);
	}

	public void SetNewsData(NewsData newsData)
	{
		_dateText.SetText(newsData.StartDate.ToString("yyyy/MM/dd HH:mm"));
		_titleText.SetText(newsData.Title);
		_bodyText.SetText(newsData.MainText);
		_scrollRect.verticalScrollbar.SetValueWithoutNotify(0f);
	}

	public void Activate()
	{
		if (_dialogAnimation == null)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			_dialogAnimation.Activate();
		}
	}

	public void Deactivate()
	{
		if (_dialogAnimation == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			_dialogAnimation.Deactivate();
		}
	}
}
