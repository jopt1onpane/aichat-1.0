using DG.Tweening;
using UnityEngine;

namespace Bulbul.Mobile;

public class TutorialFocusUIForMobile : TutorialFocusUI
{
	[SerializeField]
	[Header("maskImageはこっちのクラスでは入れなくてOKです")]
	private TutorialFocusMask _focusMask;

	[SerializeField]
	private RectTransform _focusUnMaskTarget;

	[SerializeField]
	private Vector2 _unMaskSize;

	public override void Activate(FocusType focusType)
	{
		float duration = 1f;
		float duration2 = 1f;
		switch (focusType)
		{
		case FocusType.Pomodoro:
			duration = 1.3f;
			break;
		case FocusType.Note:
		case FocusType.Todo:
			duration = 1f;
			break;
		}
		_focusMask.SetActive(active: true);
		_focusMask.SetUnMaskPosAndSize(_focusUnMaskTarget.position, _unMaskSize);
		_focusImage.gameObject.SetActive(value: true);
		_tweenRotate = _focusImage.transform.DOLocalRotate(new Vector3(0f, 0f, -360f), duration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
		if (_localScale == Vector3.zero)
		{
			_localScale = _focusImage.transform.localScale;
		}
		Vector3 endValue = _localScale * 1.1f;
		endValue.z = 1f;
		_tweenScale = _focusImage.transform.DOScale(endValue, duration2).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
	}

	public override void Activate()
	{
		float duration = 1f;
		float duration2 = 1f;
		_focusImage.gameObject.SetActive(value: true);
		_focusImage.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		_tweenRotate = _focusImage.transform.DOLocalRotate(new Vector3(0f, 0f, -360f), duration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
		if (_localScale == Vector3.zero)
		{
			_localScale = _focusImage.transform.localScale;
		}
		Vector3 endValue = _localScale * 1.1f;
		endValue.z = 1f;
		_tweenScale = _focusImage.transform.DOScale(endValue, duration2).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
	}

	public override void Deactivate()
	{
		if ((bool)_focusMask)
		{
			_focusMask.SetActive(active: false);
		}
		if ((bool)_focusImage)
		{
			_focusImage.gameObject.SetActive(value: false);
		}
		_tweenRotate?.Kill();
		_tweenScale?.Kill();
	}
}
