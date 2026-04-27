using System;
using System.Linq;
using Bulbul;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using MyUtil;
using VContainer;

public class HeroineGameEndTalk : HeroineBaseState
{
	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private LoadDirectionService _loadDirectionService;

	[Inject]
	private DebugService _debugService;

	[Inject]
	private AchievementService _achievementService;

	private GameEndTalkSelector _gameEndTalkSelector = new GameEndTalkSelector();

	public override void OnRegisterToStateMachine()
	{
		_gameEndTalkSelector.Setup();
	}

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		bool isUseDebug = false;
		ScenarioType debugScenarioType = ScenarioType.GameEnd;
		int debugEpisodeNumber = -1;
		if (_debugService != null)
		{
			isUseDebug = _debugService.IsDebugGameEndDirectionEnabled;
			debugScenarioType = _debugService.DebugGameEndDirectionScenarioType;
			debugEpisodeNumber = _debugService.DebugGameEndDirectionEpisodeNumber;
		}
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.GameEnd_Base_001);
		DelayNext().Forget();
		async UniTask DelayNext()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
			ScenarioType scenarioType;
			int episodeNumber;
			if (isUseDebug)
			{
				scenarioType = debugScenarioType;
				episodeNumber = debugEpisodeNumber;
			}
			else
			{
				ScenarioInfo scenarioInfo = _gameEndTalkSelector.TakeNextVoice();
				scenarioType = scenarioInfo.ScenarioType;
				episodeNumber = scenarioInfo.EpisodeNumber;
			}
			ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
			if (scenarioGroupData == null)
			{
				Debug.LogError($"ゲーム終了時のエピソードが見つから無かったため、最初の番号の会話を再生します。見つからなかった番号は{episodeNumber}番。タイプは{scenarioType}");
				scenarioType = ScenarioType.GameEnd;
				episodeNumber = 1;
				scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
			}
			_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto);
			_scenarioReader.StartClickHeroineReaction();
			_achievementService.OnExitCallTalk();
			_scenarioReader.AddEndCallback(delegate
			{
				EndDirection().Forget();
			});
		}
		async UniTask EndDirection()
		{
			await UniTask.WhenAny(UniTask.WaitUntil(() => base.Owner.HeroineService.IsEndAnimation(HeroineService.AnimationType.GameEnd_Base_001.ToString())), UniTask.Delay(TimeSpan.FromSeconds(1.5)));
			if (!base.Owner.HeroineService.IsEndAnimation(HeroineService.AnimationType.GameEnd_Base_001.ToString()))
			{
				await UniTask.Delay(TimeSpan.FromSeconds(0.6000000238418579));
			}
			else
			{
				await UniTask.Delay(TimeSpan.FromSeconds(0.30000001192092896));
			}
			_loadDirectionService.GameEndDirection(DevicePlatform.Steam.IsPC());
			if (isUseDebug)
			{
				_debugService.IsDebugGameEndDirectionEnabled = false;
			}
		}
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
	}

	public override bool IsPossibleClickReaction()
	{
		return true;
	}

	public override bool IsPossibleTalk()
	{
		return true;
	}

	public override void ToPossibleClickReaction()
	{
	}
}
