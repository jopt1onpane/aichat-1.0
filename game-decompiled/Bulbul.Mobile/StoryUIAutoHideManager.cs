using Bulbul.MasterData;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class StoryUIAutoHideManager : MonoBehaviour
{
	[Inject]
	private ScenarioReader _scenarioReader;

	[SerializeField]
	private ObjectsActiveChecker _facilityWindowChecker;

	[SerializeField]
	private CanvasGroup _storyUIVerticalCanvasGroup;

	[SerializeField]
	private WallPaperManagerForMobile _wallPaperManager;

	private bool _isPlayingStory;

	private bool _isWallPaper;

	private void Start()
	{
		_scenarioReader.OnStartReady.Subscribe(delegate(ScenarioType scenarioType)
		{
			if (_scenarioReader.IsPlayingLongStory())
			{
				_isPlayingStory = true;
			}
			else if (scenarioType == ScenarioType.Tutorial)
			{
				_isPlayingStory = true;
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_scenarioReader.OnEndStory, delegate
		{
			_isPlayingStory = false;
		}).AddTo(this);
		if (_wallPaperManager != null)
		{
			_wallPaperManager.OnChangedState.Subscribe(delegate(bool flag)
			{
				_isWallPaper = flag;
			}).AddTo(this);
		}
	}

	private bool CheckActiveStoryUI()
	{
		return !_facilityWindowChecker.CheckActive();
	}

	private void SetActiveStoryUIVertical(bool active)
	{
		if (_storyUIVerticalCanvasGroup.alpha > 0f != active)
		{
			if (active)
			{
				_storyUIVerticalCanvasGroup.alpha = 1f;
				_storyUIVerticalCanvasGroup.blocksRaycasts = true;
			}
			else
			{
				_storyUIVerticalCanvasGroup.alpha = 0f;
				_storyUIVerticalCanvasGroup.blocksRaycasts = false;
			}
		}
	}

	private void Update()
	{
		SetActiveStoryUIVertical(CheckActiveStoryUI() || _isPlayingStory || _isWallPaper);
	}
}
