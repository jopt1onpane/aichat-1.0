using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class NoteUIContentsView : MonoBehaviour, ITabChangedSetuper
{
	[SerializeField]
	private NoteContentsListPageView _listContent;

	[SerializeField]
	private NoteContentsInputPageView _inputContent;

	[SerializeField]
	private CanvasGroup _listPageCanvasGroup;

	[SerializeField]
	private RectTransform _inputPageRoot;

	[SerializeField]
	private CanvasGroup _inputPageCanvasGroup;

	private float _toX;

	private float _fromX;

	private Subject<bool> _onPrepare = new Subject<bool>();

	public Observable<bool> OnPrepare => _onPrepare;

	public void Setup()
	{
		_toX = _inputPageRoot.anchoredPosition.x;
		_fromX = _inputPageRoot.anchoredPosition.x + 50f;
		Vector2 anchoredPosition = _inputPageRoot.anchoredPosition;
		anchoredPosition.x = _fromX;
		_inputPageRoot.anchoredPosition = anchoredPosition;
		_inputPageCanvasGroup.gameObject.SetActive(value: false);
		_listPageCanvasGroup.gameObject.SetActive(value: true);
		_listPageCanvasGroup.alpha = 1f;
		_inputPageCanvasGroup.alpha = 0f;
		_inputPageCanvasGroup.blocksRaycasts = false;
		_inputContent.Setup();
	}

	public void EnterSetting(bool isRemovingMode)
	{
		_listContent.EnterSetting(isRemovingMode);
	}

	public void ChangeInputView(bool isStartInput, bool isTargetTitle = false)
	{
		_inputContent.ResetScroll();
		_inputPageCanvasGroup.gameObject.SetActive(value: true);
		_listPageCanvasGroup.gameObject.SetActive(value: true);
		_inputPageCanvasGroup.blocksRaycasts = false;
		_listPageCanvasGroup.blocksRaycasts = false;
		_inputPageRoot.DOAnchorPosX(_toX, 0.25f);
		_inputPageCanvasGroup.DOFade(1f, 0.25f).OnComplete(delegate
		{
			_inputPageCanvasGroup.blocksRaycasts = true;
			if (isStartInput)
			{
				if (isTargetTitle)
				{
					_inputContent.StartTitleInput();
				}
				else
				{
					_inputContent.StartMainInput();
				}
			}
		});
		_listPageCanvasGroup.DOFade(0f, 0.25f).OnComplete(delegate
		{
			_listPageCanvasGroup.gameObject.SetActive(value: false);
		});
	}

	public void ChangeNoteListView(bool isImmediate = false)
	{
		_inputPageCanvasGroup.gameObject.SetActive(value: true);
		_listPageCanvasGroup.gameObject.SetActive(value: true);
		_inputPageCanvasGroup.blocksRaycasts = false;
		_listPageCanvasGroup.blocksRaycasts = false;
		_inputPageRoot.DOAnchorPosX(_fromX, 0.25f);
		TweenerCore<float, float, FloatOptions> t = _inputPageCanvasGroup.DOFade(0f, 0.25f).OnComplete(delegate
		{
			_inputPageCanvasGroup.gameObject.SetActive(value: false);
		});
		TweenerCore<float, float, FloatOptions> t2 = _listPageCanvasGroup.DOFade(1f, 0.25f).OnComplete(delegate
		{
			_listPageCanvasGroup.blocksRaycasts = true;
		});
		if (isImmediate)
		{
			t.Complete();
			t2.Complete();
		}
	}

	void ITabChangedSetuper.SetupBeforeTabChanged(bool isChangedFromTab)
	{
		_onPrepare.OnNext(isChangedFromTab);
	}
}
