using System.Linq;
using Bulbul;
using Bulbul.MasterData;
using VContainer;

public class FacilityVoiceTextScenario
{
	private enum MainState
	{
		Idle,
		ReadyPlay,
		Playing,
		EndPlay
	}

	private MainState _mainState;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	private int _currentEpisodeNumber;

	private ScenarioType _currentScenarioType;

	public void Setup()
	{
	}

	public void UpdateFacility()
	{
		switch (_mainState)
		{
		}
	}

	public void WantPlayVoiceTextScenario(ScenarioType scenarioType, int episodeNumber)
	{
		if (!_scenarioReader.IsPlayingScenario() && (!_heroineService.IsPlayingPomodoroAction() || scenarioType == ScenarioType.SpeakWord_PomodoroStart || scenarioType == ScenarioType.SpeakWord_Pomodoro_WorkContinuousStart || scenarioType == ScenarioType.SpeakWord_Pomodoro_WorkLongStart || scenarioType == ScenarioType.SpeakWord_PomodoroBreak || scenarioType == ScenarioType.SpeakWord_Pomodoro_ShortWorkedBreakStart || scenarioType == ScenarioType.SpeakWord_Pomodoro_LongWorkedBreakStart || scenarioType == ScenarioType.SpeakWord_PomodoroFinish || scenarioType == ScenarioType.SpeakWord_Pomodoro_LongWorkFinish || scenarioType == ScenarioType.SpeakWord_Pomodoro_ShortWorkFinish || scenarioType == ScenarioType.SpeakWord_Pomodoro_MidwayFinish || scenarioType == ScenarioType.SpeakWord_LeaveChair))
		{
			_currentEpisodeNumber = episodeNumber;
			_currentScenarioType = scenarioType;
			_mainState = MainState.ReadyPlay;
		}
	}

	public bool IsStartReady()
	{
		return _mainState == MainState.ReadyPlay;
	}

	public bool IsPlayEnd()
	{
		return _mainState == MainState.EndPlay;
	}

	public void StartScenario()
	{
		ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == _currentScenarioType && x.EpisodeNumber == (float)_currentEpisodeNumber);
		if (scenarioGroupData == null)
		{
			Debug.LogError($"{_currentScenarioType}のエピソードが見つから無かったため、最初の番号の会話を再生します。見つからなかった番号は{_currentEpisodeNumber}番。");
			_currentEpisodeNumber = 1;
			scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == _currentScenarioType && x.EpisodeNumber == (float)_currentEpisodeNumber);
		}
		_scenarioReader.StartReady(_currentScenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto, isTalkLog: false, isUseMask: false);
		_scenarioReader.StartVoiceTextScenario();
		_mainState = MainState.Playing;
		_scenarioReader.AddEndCallback(delegate
		{
			if (_mainState == MainState.Playing)
			{
				_mainState = MainState.EndPlay;
			}
		});
	}

	public void EndScenario()
	{
		_mainState = MainState.Idle;
	}

	public void CancelReaction()
	{
		_mainState = MainState.Idle;
	}
}
