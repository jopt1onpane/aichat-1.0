using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class WantTalkUIForMobile : WantTalkUI
{
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private CanvasScalerReferenceResolusionSwitcher _resoSwitcher;

	[SerializeField]
	private Vector2 _portaraitAnchoredPos;

	[SerializeField]
	private Vector2 _landscapeAnchoredPos;

	private RectTransform __rectTransform;

	private Vector2 anchoredPos;

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

	public override void Setup()
	{
		if (_canvas.GetComponent<CanvasScaler>().referenceResolution.x > _canvas.GetComponent<CanvasScaler>().referenceResolution.y)
		{
			anchoredPos = _landscapeAnchoredPos;
		}
		else
		{
			anchoredPos = _portaraitAnchoredPos;
		}
		_rectTransform.anchoredPosition = CalculateAnchoredPos();
		base.Setup();
		if (!(_resoSwitcher != null))
		{
			return;
		}
		_resoSwitcher.OnSwitchedRefResolution.Subscribe(delegate(ScreenOrientation orientation)
		{
			switch (orientation)
			{
			case ScreenOrientation.LandscapeLeft:
			case ScreenOrientation.LandscapeRight:
				anchoredPos = _landscapeAnchoredPos;
				break;
			case ScreenOrientation.Portrait:
			case ScreenOrientation.PortraitUpsideDown:
				anchoredPos = _portaraitAnchoredPos;
				break;
			}
			Vector3 vector = CalculateAnchoredPos();
			_rectTransform.anchoredPosition = vector;
			_fromPosY = vector.y + -8f;
			_toPosY = vector.y;
		}).AddTo(this);
	}

	private Vector3 CalculateAnchoredPos()
	{
		return new Vector3(anchoredPos.x * (_canvas.GetComponent<RectTransform>().sizeDelta.x / 2f) / (_canvas.GetComponent<CanvasScaler>().referenceResolution.x / 2f), anchoredPos.y * (_canvas.GetComponent<RectTransform>().sizeDelta.y / 2f) / (_canvas.GetComponent<CanvasScaler>().referenceResolution.y / 2f), 0f);
	}
}
