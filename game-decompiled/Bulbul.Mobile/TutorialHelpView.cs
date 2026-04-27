using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bulbul.Mobile;

public class TutorialHelpView : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerExitHandler
{
	[SerializeField]
	private TutorialHelpPageView _pageView;

	[SerializeField]
	private Vector2 _baseScreenResolution = new Vector2(1080f, 1920f);

	[SerializeField]
	private float _swipeThresold = 20f;

	private Subject<bool> _onSwipe = new Subject<bool>();

	private Vector2 _downPos;

	private Vector2 _upPos;

	private bool _isSwipeCheck;

	public Observable<bool> OnSwipe => _onSwipe;

	public TutorialHelpPageView PageView => _pageView;

	public void Setup()
	{
		_pageView.Setup();
	}

	public void PrepareTutorialHelp(TutorialService.TutorialPageType pageType)
	{
		int pageIdx = ConvertPageTypeToIdx(pageType);
		_pageView.Prepare(pageIdx);
	}

	public void MovePage(TutorialService.TutorialPageType pageType)
	{
		int idx = ConvertPageTypeToIdx(pageType);
		_pageView.Move(idx);
	}

	private int ConvertPageTypeToIdx(TutorialService.TutorialPageType pageType)
	{
		return pageType switch
		{
			TutorialService.TutorialPageType.ScreenUI => 0, 
			TutorialService.TutorialPageType.PomodoroTimer => 1, 
			TutorialService.TutorialPageType.LevelAndStory => 2, 
			TutorialService.TutorialPageType.NewEnviroment => 3, 
			_ => 0, 
		};
	}

	private Vector2 CalcFactor()
	{
		float x = ((Screen.width <= 0) ? 1f : (_baseScreenResolution.x / (float)Screen.width));
		float y = ((Screen.height <= 0) ? 1f : (_baseScreenResolution.y / (float)Screen.height));
		return new Vector2(x, y);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		_downPos = eventData.position;
		_isSwipeCheck = true;
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		_isSwipeCheck = false;
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		_upPos = eventData.position;
		if (_isSwipeCheck)
		{
			_isSwipeCheck = false;
			float num = _upPos.x - _downPos.x;
			num *= CalcFactor().x;
			if (num >= _swipeThresold || num <= 0f - _swipeThresold)
			{
				bool value = num < 0f;
				_onSwipe.OnNext(value);
			}
		}
	}
}
