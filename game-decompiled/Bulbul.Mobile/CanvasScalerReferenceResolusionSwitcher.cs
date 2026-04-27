using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerReferenceResolusionSwitcher : MonoBehaviour
{
	[SerializeField]
	private CanvasScaler _canvasScaler;

	[SerializeField]
	private Vector2 _portraitReso;

	[SerializeField]
	private Vector2 _landscapeReso;

	private ScreenOrientation _curScreenOrientation;

	private Subject<ScreenOrientation> _onSwitchedRefResolution = new Subject<ScreenOrientation>();

	private int counter = 10;

	public Observable<ScreenOrientation> OnSwitchedRefResolution => _onSwitchedRefResolution;

	private void Start()
	{
		if (_canvasScaler == null)
		{
			_canvasScaler = GetComponent<CanvasScaler>();
		}
		Switch(Screen.orientation);
	}

	private void Update()
	{
		ScreenOrientation orientation = Screen.orientation;
		if (_curScreenOrientation != orientation)
		{
			Switch(orientation);
		}
	}

	private void Switch(ScreenOrientation orientation)
	{
		switch (orientation)
		{
		case ScreenOrientation.Portrait:
		case ScreenOrientation.PortraitUpsideDown:
			_canvasScaler.referenceResolution = _portraitReso;
			break;
		case ScreenOrientation.LandscapeLeft:
		case ScreenOrientation.LandscapeRight:
			_canvasScaler.referenceResolution = _landscapeReso;
			break;
		}
		_curScreenOrientation = orientation;
		_onSwitchedRefResolution.OnNext(_curScreenOrientation);
	}
}
