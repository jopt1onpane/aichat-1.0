using Bulbul;
using UnityEngine;

public class CursorService : MonoBehaviour
{
	[SerializeField]
	private Texture2D _defaultCursor;

	[SerializeField]
	private Texture2D _clickCursor;

	[SerializeField]
	private Texture2D _talkCursor;

	[SerializeField]
	private Texture2D _talkBlockCursor;

	private HeroineService _heroineService;

	private int _heroineLayerMask;

	private ScenarioReader _scenarioReader;

	private RoomGameManager _roomGameManager;

	private TooltipService _tooltipService;

	private SmallAnnounceService _smallAnnounceService;

	private void Awake()
	{
		_heroineLayerMask = 1 << LayerMask.NameToLayer("TouchHeroine");
		ChangeCursorDefault();
	}

	public void Setup(HeroineService heroineService, RoomGameManager roomGameManager, TooltipService tooltipService, SmallAnnounceService smallAnnounceService)
	{
		_heroineService = heroineService;
		if (_scenarioReader == null)
		{
			_scenarioReader = RoomLifetimeScope.Resolve<ScenarioReader>();
		}
		_roomGameManager = roomGameManager;
		_tooltipService = tooltipService;
		_smallAnnounceService = smallAnnounceService;
	}

	public void UpdateCursor()
	{
		if (DevicePlatform.Steam.IsMobile())
		{
			return;
		}
		UpdateTooltip();
		RaycastHit hitInfo;
		if (InputController.Instance.CurrentFrameEventSystemRaycastResult.Count > 0 || Camera.main == null)
		{
			ChangeCursorDefault();
		}
		else if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, float.PositiveInfinity, _heroineLayerMask))
		{
			if (_heroineService.IsPossibleClickHeroineReaction())
			{
				if (_scenarioReader.IsPlayingScenario() || _roomGameManager.CurrentMainState == RoomGameManager.MainState.TalkingGameStartDirection || _roomGameManager.CurrentMainState == RoomGameManager.MainState.EndGameStartDirection || _roomGameManager.CurrentMainState == RoomGameManager.MainState.Tutorial0 || _roomGameManager.CurrentMainState == RoomGameManager.MainState.Tutorial1)
				{
					if (_roomGameManager.CurrentMainState == RoomGameManager.MainState.Tutorial4)
					{
						ChangeCursorTalk();
					}
					else
					{
						ChangeCursorDefault();
					}
				}
				else
				{
					ChangeCursorTalk();
				}
			}
			else
			{
				ChangeCursorTalkBlock();
			}
		}
		else
		{
			ChangeCursorDefault();
		}
	}

	private void ChangeCursorDefault()
	{
		Cursor.SetCursor(_defaultCursor, new Vector2(7f, 0f), CursorMode.Auto);
	}

	private void ChangeCursorClick()
	{
		Cursor.SetCursor(_clickCursor, new Vector2(7f, 0f), CursorMode.Auto);
	}

	private void ChangeCursorTalk()
	{
		Cursor.SetCursor(_talkCursor, new Vector2(7f, 0f), CursorMode.Auto);
	}

	private void ChangeCursorTalkBlock()
	{
		Cursor.SetCursor(_talkBlockCursor, new Vector2(7f, 0f), CursorMode.Auto);
	}

	private void UpdateTooltip()
	{
		_tooltipService.UpdateTarget(FindTooltipTarget());
		if (InputController.Instance.GetClickUp())
		{
			_tooltipService.DeactivateOnCurrentTarget();
		}
	}

	private TooltipTarget FindTooltipTarget()
	{
		GameObject currentFrameEventSystemRaycastHitObject = InputController.Instance.CurrentFrameEventSystemRaycastHitObject;
		if (currentFrameEventSystemRaycastHitObject == null)
		{
			return null;
		}
		return currentFrameEventSystemRaycastHitObject.GetComponentInParent<TooltipTarget>();
	}
}
