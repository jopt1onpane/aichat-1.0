using System;
using System.Collections.Generic;
using Bulbul;
using UnityEngine;
using VContainer;

public class NextActionSelector : MonoBehaviour
{
	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private HeroineService _heroineService;

	[SerializeField]
	[Header("レイアウト変更する確率（%）")]
	[Range(0f, 100f)]
	public float _changeWorkLayoutRatio = 50f;

	[SerializeField]
	[Header("レイアウト変更までのDelay秒数")]
	public float _changeWorkLayoutDelaySeconds = 300f;

	[SerializeField]
	private HeroineUseObjectController _heroineUseObjectController;

	[SerializeField]
	[Header("行動切替時、\nワイルドモーションが発生する確率")]
	[Range(0f, 100f)]
	public int WildMotionOccurrenceProbability;

	[SerializeField]
	[Header("行動切替時、\nワイルドモーションが発生した後、再発生するまでのDelay分数")]
	public int WildMotionDelayMinutes;

	[Header("何分で休憩を変更するか")]
	[SerializeField]
	[Header("動画")]
	public float _changeBreakMovieTimeMinutesMin;

	[SerializeField]
	public float _changeBreakMovieTimeMinutesMax;

	[SerializeField]
	[Header("本を読む")]
	public float _changeBreakBookTimeMinutesMin;

	[SerializeField]
	public float _changeBreakBookTimeMinutesMax;

	[SerializeField]
	[Header("音楽を聴く")]
	public float _changeBreakMusicTimeMinutesMin;

	[SerializeField]
	public float _changeBreakMusicTimeMinutesMax;

	[SerializeField]
	[Header("お茶")]
	public float _changeBreakTeaTimeMinutesMin;

	[SerializeField]
	public float _changeBreakTeaTimeMinutesMax;

	[SerializeField]
	[Header("睡眠")]
	public float _changeBreakSleepTimeMinutesMin;

	[SerializeField]
	public float _changeBreakSleepTimeMinutesMax;

	[SerializeField]
	[Header("前傾姿勢休憩")]
	public float _changeBreakForwardTimeMinutesMin;

	[SerializeField]
	public float _changeBreakForwardTimeMinutesMax;

	[SerializeField]
	[Header("睡眠が発生する確率")]
	[Range(0f, 100f)]
	public int _breakSleepOccurrenceProbability;

	[SerializeField]
	[Header("休憩中のヒロインクリック時、\nモーション変更が可能になる秒数")]
	public int _clickChangeBreakDelayTimeSeconds = 20;

	[SerializeField]
	[Header("モーションを変更する確率")]
	[Range(0f, 100f)]
	public float _clickChangeBreakProbability = 50f;

	private List<HeroineAI.ActionStateType> _candidateWorkList;

	private HeroineAI.ActionStateType _currentWork = HeroineAI.ActionStateType.WorkPC;

	private HeroineAI.ActionStateType _nextWork = HeroineAI.ActionStateType.None;

	private DateTime _startWorkDateTime;

	private List<HeroineAI.ActionStateType> _candidateBreakList;

	private HeroineAI.ActionStateType _currentBreak = HeroineAI.ActionStateType.None;

	private HeroineAI.ActionStateType _nextBreak = HeroineAI.ActionStateType.None;

	private DateTime _startBreakDateTime;

	private float _changeBreakTimeMinutes;

	public DateTime LastUseWildMotionDateTime = DateTime.MinValue;

	private bool _isNeedChangeLayoutAction;

	private MyStopWatch _changeLayoutDelayStopWatch = new MyStopWatch();

	public DateTime BreakStartDateTime => _startBreakDateTime;

	public void Setup()
	{
		InitCandidateWorkList();
		UpdateNextWorkAction();
		SetupCandidateBreakList();
		UpdateNextBreakAction();
		_changeLayoutDelayStopWatch.SetTargetSeconds(_changeWorkLayoutDelaySeconds);
		_changeLayoutDelayStopWatch.Watch.Start();
	}

	private void InitCandidateWorkList()
	{
		_candidateWorkList = new List<HeroineAI.ActionStateType>
		{
			HeroineAI.ActionStateType.WorkPC,
			HeroineAI.ActionStateType.WorkBook,
			HeroineAI.ActionStateType.WorkReport
		};
		ShuffleUtils.Shuffle(_candidateWorkList);
	}

	private void SetupCandidateBreakList()
	{
		_candidateBreakList = new List<HeroineAI.ActionStateType>
		{
			HeroineAI.ActionStateType.BreakMovie,
			HeroineAI.ActionStateType.BreakReadBook,
			HeroineAI.ActionStateType.BreakTeaTime
		};
		ShuffleUtils.Shuffle(_candidateBreakList);
	}

	private void InitCandidateBreakList()
	{
		_candidateBreakList = new List<HeroineAI.ActionStateType>
		{
			HeroineAI.ActionStateType.BreakMovie,
			HeroineAI.ActionStateType.BreakReadBook,
			HeroineAI.ActionStateType.BreakListenMusic,
			HeroineAI.ActionStateType.BreakTeaTime,
			HeroineAI.ActionStateType.BreakSleep,
			HeroineAI.ActionStateType.BreakForward
		};
		ShuffleUtils.Shuffle(_candidateBreakList);
	}

	public HeroineAI.ActionStateType GetNextAction(HeroineAI.ActionType nextActionType)
	{
		HeroineAI.ActionStateType result = HeroineAI.ActionStateType.None;
		switch (nextActionType)
		{
		case HeroineAI.ActionType.Work:
			if (IsSameCurrentDeskType(_nextWork))
			{
				result = _nextWork;
				break;
			}
			switch (_heroineUseObjectController.DeskType)
			{
			case DeskType.Book:
				result = HeroineAI.ActionStateType.WorkBook;
				break;
			case DeskType.Pc:
				result = HeroineAI.ActionStateType.WorkPC;
				break;
			case DeskType.Report:
				result = HeroineAI.ActionStateType.WorkReport;
				break;
			default:
				Debug.LogError($"定義されていない机の状態{_heroineUseObjectController.DeskType}の為、一旦現在の作業を継続。");
				result = _currentWork;
				break;
			}
			break;
		case HeroineAI.ActionType.Break:
			if (_heroineService.IsNeedChangeWantTalk())
			{
				return HeroineAI.ActionStateType.WantTalk;
			}
			result = _nextBreak;
			break;
		}
		return result;
	}

	public void UpdateNextWorkAction()
	{
		_nextWork = _currentWork;
		float num = UnityEngine.Random.Range(0f, 100f);
		if ((_currentWork == HeroineAI.ActionStateType.None || num <= _changeWorkLayoutRatio) && _changeLayoutDelayStopWatch.IsElapsedTargetTime())
		{
			_nextWork = GetNextWorkAction();
			_isNeedChangeLayoutAction = true;
			_changeLayoutDelayStopWatch.Watch.Restart();
		}
	}

	public void UpdateNextBreakAction(bool isClickEnd = false)
	{
		if (isClickEnd)
		{
			_nextBreak = GetNextBreakAction();
			if (_nextBreak == HeroineAI.ActionStateType.BreakSleep)
			{
				UseNextBreakAction(isClickEnd);
				_nextBreak = GetNextBreakAction();
				if (_nextBreak == HeroineAI.ActionStateType.BreakSleep)
				{
					UseNextBreakAction(isClickEnd);
					_nextBreak = GetNextBreakAction();
				}
			}
		}
		else
		{
			TimeSpan timeSpan = DateTime.Now - _startBreakDateTime;
			if (_currentBreak == HeroineAI.ActionStateType.None || timeSpan.TotalMinutes >= (double)_changeBreakTimeMinutes)
			{
				_nextBreak = GetNextBreakAction();
			}
			else
			{
				_nextBreak = _currentBreak;
			}
		}
		if (_nextBreak == HeroineAI.ActionStateType.BreakForward && _heroineUseObjectController.DeskType != DeskType.Pc)
		{
			UseNextBreakAction(isClickEnd);
			_nextBreak = GetNextBreakAction();
			if (_nextBreak == HeroineAI.ActionStateType.BreakSleep)
			{
				UseNextBreakAction(isClickEnd);
				_nextBreak = GetNextBreakAction();
			}
		}
	}

	public bool IsPossibleChangeBreakType()
	{
		if (_heroineService.IsLeaveChair())
		{
			return false;
		}
		if (!_heroineService.LookDirectionService.IsLookInitDirection())
		{
			return false;
		}
		return (DateTime.Now - _startBreakDateTime).TotalMinutes >= (double)_changeBreakTimeMinutes;
	}

	public void WantChangeBreakAction(bool isClickEnd = false)
	{
		if (!isClickEnd || (!((DateTime.Now - _startBreakDateTime).TotalSeconds < (double)_clickChangeBreakDelayTimeSeconds) && (float)UnityEngine.Random.Range(1, 101) <= _clickChangeBreakProbability))
		{
			_changeBreakTimeMinutes = 0f;
		}
	}

	private HeroineAI.ActionStateType GetNextBreakAction()
	{
		if (_candidateBreakList[0] == HeroineAI.ActionStateType.BreakSleep && UnityEngine.Random.Range(1, 101) > _breakSleepOccurrenceProbability)
		{
			_candidateBreakList.RemoveAt(0);
			if (_candidateBreakList.Count <= 0)
			{
				InitCandidateBreakList();
			}
		}
		return _candidateBreakList[0];
	}

	public void UseNextBreakAction(bool isClickEnd = false)
	{
		if (_heroineService.IsNeedChangeWantTalk())
		{
			return;
		}
		if (isClickEnd)
		{
			UseAction();
			return;
		}
		TimeSpan timeSpan = DateTime.Now - _startBreakDateTime;
		if (_currentBreak == HeroineAI.ActionStateType.None || timeSpan.TotalMinutes >= (double)_changeBreakTimeMinutes)
		{
			UseAction();
		}
		void UseAction()
		{
			_startBreakDateTime = DateTime.Now;
			_currentBreak = GetNextBreakAction();
			_candidateBreakList.RemoveAt(0);
			if (_candidateBreakList.Count <= 0)
			{
				InitCandidateBreakList();
			}
			float minInclusive = 0f;
			float maxInclusive = 0f;
			switch (_currentBreak)
			{
			case HeroineAI.ActionStateType.BreakMovie:
				minInclusive = _changeBreakMovieTimeMinutesMin;
				maxInclusive = _changeBreakMovieTimeMinutesMax;
				break;
			case HeroineAI.ActionStateType.BreakReadBook:
				minInclusive = _changeBreakBookTimeMinutesMin;
				maxInclusive = _changeBreakBookTimeMinutesMax;
				break;
			case HeroineAI.ActionStateType.BreakListenMusic:
				minInclusive = _changeBreakMusicTimeMinutesMin;
				maxInclusive = _changeBreakMusicTimeMinutesMax;
				break;
			case HeroineAI.ActionStateType.BreakTeaTime:
				minInclusive = _changeBreakTeaTimeMinutesMin;
				maxInclusive = _changeBreakTeaTimeMinutesMax;
				break;
			case HeroineAI.ActionStateType.BreakSleep:
				minInclusive = _changeBreakSleepTimeMinutesMin;
				maxInclusive = _changeBreakSleepTimeMinutesMax;
				break;
			case HeroineAI.ActionStateType.BreakForward:
				minInclusive = _changeBreakForwardTimeMinutesMin;
				maxInclusive = _changeBreakForwardTimeMinutesMax;
				break;
			}
			_changeBreakTimeMinutes = UnityEngine.Random.Range(minInclusive, maxInclusive);
		}
	}

	private HeroineAI.ActionStateType GetNextWorkAction()
	{
		return _candidateWorkList[0];
	}

	public void UseNextWorkAction()
	{
		_ = DateTime.Now - _startWorkDateTime;
		if (_currentWork == HeroineAI.ActionStateType.None || _isNeedChangeLayoutAction)
		{
			_isNeedChangeLayoutAction = false;
			_startWorkDateTime = DateTime.Now;
			_currentWork = GetNextWorkAction();
			_candidateWorkList.RemoveAt(0);
			if (_candidateWorkList.Count <= 0)
			{
				InitCandidateWorkList();
			}
		}
	}

	public HeroineAI.ActionStateType GetNextWildState()
	{
		HeroineAI.ActionStateType actionStateType = HeroineAI.ActionStateType.None;
		if (_pomodoroService.CurrentPomodoroType == PomodoroService.PomodoroType.Work)
		{
			bool flag = false;
			switch (_heroineUseObjectController.DeskType)
			{
			case DeskType.Book:
				if (_nextWork != HeroineAI.ActionStateType.WorkBook)
				{
					flag = true;
				}
				break;
			case DeskType.Pc:
				if (_nextWork != HeroineAI.ActionStateType.WorkPC)
				{
					flag = true;
				}
				break;
			case DeskType.Report:
				if (_nextWork != HeroineAI.ActionStateType.WorkReport)
				{
					flag = true;
				}
				break;
			}
			if (flag && _currentWork != _nextWork)
			{
				switch (_currentWork)
				{
				case HeroineAI.ActionStateType.WorkPC:
					switch (_nextWork)
					{
					case HeroineAI.ActionStateType.WorkBook:
						actionStateType = HeroineAI.ActionStateType.FromPcToBook;
						break;
					case HeroineAI.ActionStateType.WorkReport:
						actionStateType = HeroineAI.ActionStateType.FromPcToReport;
						break;
					}
					break;
				case HeroineAI.ActionStateType.WorkBook:
					switch (_nextWork)
					{
					case HeroineAI.ActionStateType.WorkPC:
						actionStateType = HeroineAI.ActionStateType.FromBookToPc;
						break;
					case HeroineAI.ActionStateType.WorkReport:
						actionStateType = HeroineAI.ActionStateType.FromBookToReport;
						break;
					}
					break;
				case HeroineAI.ActionStateType.WorkReport:
					switch (_nextWork)
					{
					case HeroineAI.ActionStateType.WorkPC:
						actionStateType = HeroineAI.ActionStateType.FromReportToPc;
						break;
					case HeroineAI.ActionStateType.WorkBook:
						actionStateType = HeroineAI.ActionStateType.FromReportToBook;
						break;
					}
					break;
				}
			}
			if (_nextWork == _currentWork)
			{
				UseNextWorkAction();
			}
			if (actionStateType != HeroineAI.ActionStateType.None)
			{
				return actionStateType;
			}
		}
		if (UnityEngine.Random.Range(1, 101) <= WildMotionOccurrenceProbability)
		{
			if (_heroineService.HeroineUseObjectController.IsCanOpenOrCloseWindow())
			{
				return _heroineService.HeroineUseObjectController.GetNextOpenOrCloseState();
			}
			if ((DateTime.Now - LastUseWildMotionDateTime).TotalMinutes >= (double)WildMotionDelayMinutes)
			{
				LastUseWildMotionDateTime = DateTime.Now;
				return (HeroineAI.ActionStateType)UnityEngine.Random.Range(4, 7);
			}
		}
		else
		{
			actionStateType = HeroineAI.ActionStateType.None;
		}
		return actionStateType;
	}

	private bool IsSameCurrentDeskType(HeroineAI.ActionStateType actionStateType)
	{
		bool result = false;
		switch (_heroineUseObjectController.DeskType)
		{
		case DeskType.Book:
			if (actionStateType == HeroineAI.ActionStateType.WorkBook)
			{
				result = true;
			}
			break;
		case DeskType.Pc:
			if (actionStateType == HeroineAI.ActionStateType.WorkPC)
			{
				result = true;
			}
			break;
		case DeskType.Report:
			if (actionStateType == HeroineAI.ActionStateType.WorkReport)
			{
				result = true;
			}
			break;
		default:
			Debug.LogError($"定義されていない机の状態{_heroineUseObjectController.DeskType}です。");
			break;
		}
		return result;
	}
}
