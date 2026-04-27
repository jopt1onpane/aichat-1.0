using System;
using System.Linq;
using System.Threading;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using MyUtil;
using VContainer;

namespace Bulbul;

public class HeroineStateEventNewYearCountdown : HeroineBaseState
{
	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private FacilityLockEventService _facilityLockEventService;

	private bool _isPlaying;

	private DateTime _eventStartDateTime;

	public override void OnRegisterToStateMachine()
	{
		_eventStartDateTime = new DateTime(2026, 1, 1, 0, 0, 0).Subtract(TimeSpan.FromSeconds(10.0));
	}

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		_facilityLockEventService.SendLockEvent();
		_isPlaying = false;
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Base001);
	}

	protected override void OnUpdate()
	{
		if (!_isPlaying && CanStartEvent())
		{
			StartScenario();
		}
	}

	private bool CanStartEvent()
	{
		if (base.Owner.HeroineService.IsCurrentAnimation(HeroineService.AnimationType.Base001.ToString()))
		{
			if (DateTime.Now >= _eventStartDateTime)
			{
				return true;
			}
		}
		else
		{
			base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Base001);
		}
		return false;
	}

	private void StartScenario()
	{
		_isPlaying = true;
		ScenarioType scenarioType = ScenarioType.Event_2025_NewYear_Countdown;
		ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == 1f);
		_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto, isTalkLog: false, isUseMask: false);
		_scenarioReader.StartVoiceTextScenario();
		EventEndScenario().Forget();
		async UniTask EventEndScenario()
		{
			UniTask uniTask = UniTask.WaitUntil(() => !_scenarioReader.IsPlayingScenario());
			await UniTask.WhenAny(uniTask);
			base.Owner.ChangeCurrentMatcheAction();
		}
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
		_facilityLockEventService.SendUnlockEvent();
		_isPlaying = false;
	}

	public override bool IsPossibleClickReaction()
	{
		return false;
	}

	public override bool IsPossibleTalk()
	{
		return false;
	}

	public override void ToPossibleClickReaction()
	{
	}

	public override async UniTask ReadyFinishState(CancellationToken token)
	{
	}
}
