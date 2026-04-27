using Bulbul;
using Bulbul.MasterData;
using R3;
using UnityEngine;
using VContainer;

public class DebugService : MonoBehaviour
{
	[Inject]
	private FacilityClickHeroine _facilityClickHeroine;

	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private PomodoroTimerService _pomodoroTimerService;

	[Inject]
	private FacilityVoiceTextScenario _facilityVoiceTextScenario;

	[Inject]
	private PhotographyToolService _photographyToolService;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private AudioMixerGroupContainer _audioMixerGroupContainer;

	[Inject]
	private NoteService _noteService;

	[Inject]
	private PlayerPointService _pointService;

	[Inject]
	private IGameDemoView _gameDemoView;

	[Inject]
	private RoomCameraManager _roomCameraManager;

	[SerializeField]
	private RoomGameManager _roomGameManager;

	private ReactiveProperty<bool> _isUseDebug = new ReactiveProperty<bool>(value: false);

	private bool _isAdjustAllVolume;

	public ReadOnlyReactiveProperty<bool> IsUseDebug => _isUseDebug;

	public string ScenarioCheckStartLabel { get; set; } = string.Empty;

	public bool IsDebugPomodoroVoiceEnabled { get; set; }

	public bool IsDebugPomodoroVoicePlayEnabled { get; set; }

	public bool IsDebugPomodoroLongWorkStartVoicePlayEnabled { get; set; }

	public bool IsDebugPomodoroContinuousWorkVoicePlayEnabled { get; set; }

	public bool IsDebugPomodoroShortWorkedBreakStartVoicePlayEnabled { get; set; }

	public bool IsDebugPomodoroLongWorkedBreakStartVoicePlayEnabled { get; set; }

	public bool IsDebugPomodoroLongWorkFinishVoicePlayEnabled { get; set; }

	public bool IsDebugPomodoroShortWorkFinishVoicePlayEnabled { get; set; }

	public bool IsDebugPomodoroMidwayFinishVoicePlayEnabled { get; set; }

	public int DebugPomodoroWorkVoiceEpisodeNumber { get; set; } = 1;

	public int DebugPomodoroBreakVoiceEpisodeNumber { get; set; } = 1;

	public int DebugPomodoroFinishVoiceEpisodeNumber { get; set; } = 1;

	public bool IsDebugPomodoroMotionEnabled { get; set; }

	public HeroineAI.ActionStateType DebugPomodoroStartMotionType { get; set; } = HeroineAI.ActionStateType.WildStretchFullBody;

	public HeroineAI.ActionStateType DebugPomodoroBreakMotionType { get; set; } = HeroineAI.ActionStateType.WildStretchFullBody;

	public HeroineAI.ActionStateType DebugPomodoroFinishMotionType { get; set; } = HeroineAI.ActionStateType.WildStretchFullBody;

	public bool IsUsePhotographyDebugEnabled { get; set; }

	public float PhotographyStartTime { get; set; }

	public bool IsUseManageShowUI { get; set; }

	public LeaveChairKitchenSoundKind LeaveChairKitchenSoundChoice { get; set; }

	public HeroineCupcake.CupcakeKind CupcakeKind { get; set; }

	public bool IsDebugGameStartDirectionEnabled { get; set; }

	public GameStartDirectionScenarioType DebugGameStartDirectionScenarioType { get; set; }

	public int DebugGameStartDirectionEpisodeNumber { get; set; } = 1;

	public bool IsDebugGameEndDirectionEnabled { get; set; }

	public bool IsNeedTidyingGameEndDirection { get; set; }

	public ScenarioType DebugGameEndDirectionScenarioType { get; set; } = ScenarioType.GameEnd;

	public int DebugGameEndDirectionEpisodeNumber { get; set; } = 1;
}
