using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class CommonHideUIController : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup[] _hideObjs;

	[SerializeField]
	private Button _hideButton;

	private Sequence _sequence;

	private bool _isHide;

	public void Start()
	{
		ObservableSubscribeExtensions.Subscribe(_hideButton.OnClickAsObservable(), delegate
		{
			if (!_isHide)
			{
				Hide();
			}
		}).AddTo(this);
	}

	private void Update()
	{
		if (_isHide && Input.GetMouseButton(0))
		{
			Show();
		}
	}

	private void OnDisable()
	{
		Show();
		_sequence.Complete();
	}

	private void SetBlockRaycast(bool active)
	{
		CanvasGroup[] hideObjs = _hideObjs;
		for (int i = 0; i < hideObjs.Length; i++)
		{
			hideObjs[i].blocksRaycasts = active;
		}
	}

	public void Hide()
	{
		_sequence?.Kill();
		SetBlockRaycast(active: false);
		_sequence = DOTween.Sequence();
		CanvasGroup[] hideObjs = _hideObjs;
		foreach (CanvasGroup target in hideObjs)
		{
			_sequence.Join(target.DOFade(0f, 0.2f));
		}
		_sequence.OnComplete(delegate
		{
			SetBlockRaycast(active: true);
		});
		_isHide = true;
	}

	public void Show()
	{
		_sequence?.Kill();
		SetBlockRaycast(active: false);
		_sequence = DOTween.Sequence();
		CanvasGroup[] hideObjs = _hideObjs;
		foreach (CanvasGroup target in hideObjs)
		{
			_sequence.Join(target.DOFade(1f, 0.2f));
		}
		_sequence.OnComplete(delegate
		{
			SetBlockRaycast(active: true);
		});
		_isHide = false;
	}
}
