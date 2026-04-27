using System.Linq;
using Bulbul.MasterData;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class FacilityClickHeroine : MonoBehaviour
{
	private enum MainState
	{
		Idle,
		ReadyReaction,
		PlayingReaction,
		EndPlayReaction
	}

	public enum ReactionType
	{
		Click,
		HeroineSelf
	}

	[Inject]
	private AchievementService _achievementService;

	private MainState _mainState;

	private ReactionType _reactionType;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private ApplicationFocusService _applicationFocusService;

	[Inject]
	private SmallTalkSelector _smallTalkSelector;

	[Inject]
	private TimeOfDayProvider _timeOfDayProvider;

	private int _heroineLayerMask;

	private ReactionTalkSelector _clickReactionTalkSelector = new ReactionTalkSelector();

	private Subject<Vector2> _onUnableReactionToTap = new Subject<Vector2>();

	public Observable<Vector2> OnUnableReactionToTap => _onUnableReactionToTap;

	public bool IsClickedHeroineCurrentFrame { get; set; }

	private void OnDestroy()
	{
		_onUnableReactionToTap.Dispose();
	}

	public void Setup()
	{
		_heroineLayerMask = 1 << LayerMask.NameToLayer("TouchHeroine");
		_clickReactionTalkSelector.Setup();
	}

	public void UpdateFacility()
	{
		UpdateHeroineClickFlag();
		switch (_mainState)
		{
		case MainState.Idle:
			ClickHeroine();
			break;
		case MainState.ReadyReaction:
		case MainState.PlayingReaction:
		case MainState.EndPlayReaction:
			break;
		}
	}

	private void UpdateHeroineClickFlag()
	{
		IsClickedHeroineCurrentFrame = false;
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			IsClickedHeroineCurrentFrame = Physics.Raycast(ray, out var _, float.PositiveInfinity, _heroineLayerMask);
		}
	}

	private void ClickHeroine()
	{
		if (_applicationFocusService.IsJustAfterFocusChanged())
		{
			Input.GetMouseButtonDown(0);
		}
		else if (Input.GetMouseButtonDown(0))
		{
			if (InputController.Instance.CurrentFrameEventSystemRaycastResult.Count > 0)
			{
				_ = InputController.Instance.CurrentFrameEventSystemRaycastHitObject?.name;
			}
			else if (IsClickedHeroineCurrentFrame && !ReactionReady(ReactionType.Click))
			{
				_onUnableReactionToTap.OnNext(Input.mousePosition);
			}
		}
	}

	public bool ReactionReady(ReactionType reactionType)
	{
		if (!_heroineService.IsPossibleClickHeroineReaction())
		{
			return false;
		}
		if (_mainState != MainState.Idle)
		{
			return false;
		}
		if (reactionType == ReactionType.HeroineSelf && _heroineService.GetCurrentAIState() == HeroineAI.ActionStateType.BreakSleep)
		{
			return false;
		}
		_reactionType = reactionType;
		_mainState = MainState.ReadyReaction;
		return true;
	}

	public bool IsReactionStartReady()
	{
		return _mainState == MainState.ReadyReaction;
	}

	public bool IsReactionPlayEnd()
	{
		return _mainState == MainState.EndPlayReaction;
	}

	public void StartReaction(ScenarioType scenarioType)
	{
		switch (_reactionType)
		{
		case ReactionType.Click:
			if (scenarioType == ScenarioType.Event_2026_AprilFool)
			{
				if (PlayAprilFoolTalk())
				{
					break;
				}
				Debug.LogError("エイプリルフールのシナリオが見つから無かったため、通常のリアクションを行います。");
			}
			if (!_pomodoroService.IsTimerRunning())
			{
				float num = -1f;
				if (scenarioType == ScenarioType.HeroineClickNormal)
				{
					num = _smallTalkSelector.LotterySmallTalk(SmallTalkSelector.PlayTiming.Break);
					if (num != -1f)
					{
						if (PlaySmallTalk(num))
						{
							break;
						}
						Debug.LogError("ヒロインクリック時の小話エピソードが見つから無かったため、通常のリアクションを行います。");
					}
				}
			}
			if (_pomodoroService.IsChoiceTalkAvailable())
			{
				if (Random.Range(0f, 1f) <= _pomodoroService.GetAnswerChoiceProbability())
				{
					AnswerChoiceStory();
					_pomodoroService.UseChoiceTalk();
				}
				else
				{
					OneWord(scenarioType);
				}
			}
			else
			{
				OneWord(scenarioType);
			}
			break;
		case ReactionType.HeroineSelf:
			if (!_pomodoroService.IsCurrentWorking())
			{
				OneWord(scenarioType);
			}
			break;
		}
	}

	public bool PlaySmallTalk(float episodeNo)
	{
		ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == ScenarioType.SmallTalk && x.EpisodeNumber == episodeNo);
		if (scenarioGroupData != null)
		{
			_scenarioReader.StartReady(ScenarioType.SmallTalk, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto, isTalkLog: false, isUseMask: false);
			_scenarioReader.StartClickHeroineReaction();
			_smallTalkSelector.OnStartTalk();
			_mainState = MainState.PlayingReaction;
			_scenarioReader.AddEndCallback(delegate
			{
				if (_mainState == MainState.PlayingReaction)
				{
					_mainState = MainState.EndPlayReaction;
				}
			});
			return true;
		}
		return false;
	}

	public bool PlayAprilFoolTalk()
	{
		ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == ScenarioType.Event_2026_AprilFool && x.EpisodeNumber == 1f);
		if (scenarioGroupData != null)
		{
			_scenarioReader.StartReady(ScenarioType.Event_2026_AprilFool, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto, isTalkLog: false, isUseMask: false);
			_scenarioReader.StartClickHeroineReaction();
			_smallTalkSelector.OnStartTalk();
			_mainState = MainState.PlayingReaction;
			_scenarioReader.AddEndCallback(delegate
			{
				if (_mainState == MainState.PlayingReaction)
				{
					_mainState = MainState.EndPlayReaction;
				}
			});
			return true;
		}
		return false;
	}

	public void OneWord(ScenarioType scenarioType)
	{
		int episodeNumber = 0;
		switch (scenarioType)
		{
		case ScenarioType.HeroineClickNormal:
			switch (_reactionType)
			{
			case ReactionType.Click:
				episodeNumber = _clickReactionTalkSelector.GetNextNormalClickShortTalk();
				_clickReactionTalkSelector.UseNextNormalClickShortTalk();
				break;
			case ReactionType.HeroineSelf:
				(scenarioType, episodeNumber) = _clickReactionTalkSelector.GetNextBreakHeroineSelfShortTalkSelection(_timeOfDayProvider.GetCurrentTimeOfDayType());
				break;
			}
			break;
		case ScenarioType.HeroineClickWork:
			(scenarioType, episodeNumber) = _clickReactionTalkSelector.GetNextWorkClickShortTalkSelection(_timeOfDayProvider.GetCurrentTimeOfDayType());
			_achievementService.OnStartTalkWorking();
			break;
		case ScenarioType.HeroineClickBreak:
			switch (_reactionType)
			{
			case ReactionType.Click:
				(scenarioType, episodeNumber) = _clickReactionTalkSelector.GetNextBreakClickShortTalkSelection(_timeOfDayProvider.GetCurrentTimeOfDayType());
				_achievementService.OnStartTalkBreaking();
				break;
			case ReactionType.HeroineSelf:
				(scenarioType, episodeNumber) = _clickReactionTalkSelector.GetNextBreakHeroineSelfShortTalkSelection(_timeOfDayProvider.GetCurrentTimeOfDayType());
				break;
			}
			break;
		}
		ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
		if (scenarioGroupData == null)
		{
			Debug.LogError($"ヒロインクリック時のエピソードが見つから無かったため、最初の番号の会話を再生します。見つからなかった番号は{episodeNumber}番。タイプは{scenarioType}");
			episodeNumber = 1;
			scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
		}
		_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto, isTalkLog: false, isUseMask: false);
		_scenarioReader.StartClickHeroineReaction();
		_mainState = MainState.PlayingReaction;
		_scenarioReader.AddEndCallback(delegate
		{
			if (_mainState == MainState.PlayingReaction)
			{
				_mainState = MainState.EndPlayReaction;
			}
		});
	}

	public void OneWord(ScenarioType scenarioType, int episodeNumber)
	{
		switch (scenarioType)
		{
		default:
			Debug.LogError($"OneWordに{scenarioType}は定義されていません。");
			break;
		case ScenarioType.HeroineClickNormal:
		case ScenarioType.HeroineClickWork:
		case ScenarioType.HeroineClickBreak:
		case ScenarioType.HeroineSelfShortTalkBreak:
		case ScenarioType.HeroineClickWork_Morning:
		case ScenarioType.HeroineClickWork_Noon:
		case ScenarioType.HeroineClickWork_Evening:
		case ScenarioType.HeroineClickWork_Night:
		case ScenarioType.HeroineClickBreak_Morning:
		case ScenarioType.HeroineClickBreak_Noon:
		case ScenarioType.HeroineClickBreak_Evening:
		case ScenarioType.HeroineClickBreak_Night:
		case ScenarioType.HeroineSelfShortTalkBreak_Morning:
		case ScenarioType.HeroineSelfShortTalkBreak_Noon:
		case ScenarioType.HeroineSelfShortTalkBreak_Evening:
		case ScenarioType.HeroineSelfShortTalkBreak_Night:
		case ScenarioType.HeroineSelfShortTalk_CurrentTime:
		case ScenarioType.HeroineSelfShortTalk_WorkedTime:
			break;
		}
		ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
		if (scenarioGroupData == null)
		{
			Debug.LogError($"ヒロインクリック時のエピソードが見つから無かったため、最初の番号の会話を再生します。見つからなかった番号は{episodeNumber}番。");
			episodeNumber = 1;
			scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
		}
		_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto, isTalkLog: false, isUseMask: false);
		_scenarioReader.StartClickHeroineReaction();
		_mainState = MainState.PlayingReaction;
		_scenarioReader.AddEndCallback(delegate
		{
			if (_mainState == MainState.PlayingReaction)
			{
				_mainState = MainState.EndPlayReaction;
			}
		});
	}

	public void AnswerChoiceStory()
	{
		ScenarioType scenarioType = ScenarioType.ShortConversation;
		int episodeNumber = _clickReactionTalkSelector.GetNextAnswerChoiceEpisode();
		_clickReactionTalkSelector.UseNextAnswerChoiceEpisode();
		ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
		if (scenarioGroupData == null)
		{
			Debug.LogError($"ポモドーロ終了時のエピソードが見つから無かったため、最初の番号の会話を再生します。見つからなかった番号は{episodeNumber}番。");
			episodeNumber = 1;
			scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
		}
		_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto, isTalkLog: false, isUseMask: false);
		_scenarioReader.StartClickHeroineReaction();
		_mainState = MainState.PlayingReaction;
		_scenarioReader.AddEndCallback(delegate
		{
			_achievementService.OnEndTalkAfterWork();
			if (_mainState == MainState.PlayingReaction)
			{
				_mainState = MainState.EndPlayReaction;
			}
		});
	}

	public void AnswerChoiceStory(int episodeNumber)
	{
		ScenarioType scenarioType = ScenarioType.ShortConversation;
		ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
		if (scenarioGroupData == null)
		{
			Debug.LogError($"ポモドーロ終了時のエピソードが見つから無かったため、最初の番号の会話を再生します。見つからなかった番号は{episodeNumber}番。");
			episodeNumber = 1;
			scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
		}
		_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto, isTalkLog: false, isUseMask: false);
		_scenarioReader.StartClickHeroineReaction();
		_mainState = MainState.PlayingReaction;
		_scenarioReader.AddEndCallback(delegate
		{
			if (_mainState == MainState.PlayingReaction)
			{
				_mainState = MainState.EndPlayReaction;
			}
		});
	}

	public void EndReaction()
	{
		_scenarioReader.EndClickHeroineReaction();
		if (_scenarioReader.PlayingScenarioType == ScenarioType.SmallTalk)
		{
			_smallTalkSelector.OnEndTalk();
		}
		_mainState = MainState.Idle;
	}

	public void CancelReaction()
	{
		_mainState = MainState.Idle;
	}

	public void EndReactionForTutorial()
	{
		_mainState = MainState.Idle;
	}
}
