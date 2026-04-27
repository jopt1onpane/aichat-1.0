using R3;
using TMPro;
using UnityEngine;

namespace Bulbul.Mobile;

public class AutoSizingHeightInputFieldView : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private float _minHeight;

	private RectTransform __rectTransform;

	private Subject<(float, float, bool)> _onValueChangedHeight = new Subject<(float, float, bool)>();

	private float _prevHeight;

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

	public float Height => _rectTransform.rect.height;

	public Observable<string> OnValueChanged => _inputField.OnValueChangedAsObservable();

	public Observable<string> OnEndEdit => _inputField.OnEndEditAsObservable();

	public Observable<(float, float, bool)> OnValueChangedHeight => _onValueChangedHeight;

	private void Start()
	{
		_prevHeight = _rectTransform.rect.height;
	}

	private void OnDestroy()
	{
		_onValueChangedHeight.Dispose();
	}

	public void SetText(string str, bool isNoticeChagedText = false, bool isChangeHeight = false)
	{
		if (isNoticeChagedText)
		{
			_inputField.text = str;
		}
		else
		{
			_inputField.SetTextWithoutNotify(str);
		}
		CalcHeight(isChangeHeight, isFromSetText: true);
	}

	public void LateUpdate()
	{
		CalcHeight();
	}

	private void CalcHeight(bool isNotice = true, bool isFromSetText = false)
	{
		float preferredHeight = _inputField.preferredHeight;
		float num = ((preferredHeight > _minHeight) ? preferredHeight : _minHeight);
		Vector2 sizeDelta = _rectTransform.sizeDelta;
		sizeDelta.y = num;
		_rectTransform.sizeDelta = sizeDelta;
		float num2 = _prevHeight - num;
		if (((double)num2 > 0.001 || (double)num2 < -0.001) && isNotice)
		{
			_onValueChangedHeight.OnNext((_prevHeight, sizeDelta.y, isFromSetText));
		}
		_prevHeight = sizeDelta.y;
	}

	public void ActivateInputField()
	{
		_inputField.ActivateInputField();
	}
}
