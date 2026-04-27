using System;
using System.ComponentModel;
using Bulbul;
using Bulbul.Achievements;
using Bulbul.MasterData;
using GUPS.Obfuscator.Attribute;

[DoNotObfuscateClass]
public class SROptions : INotifyPropertyChanged
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public sealed class DisplayNameAttribute : System.ComponentModel.DisplayNameAttribute
	{
		public DisplayNameAttribute(string displayName)
			: base(displayName)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public sealed class IncrementAttribute : Attribute
	{
		public IncrementAttribute(double increment)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public sealed class NumberRangeAttribute : Attribute
	{
		public NumberRangeAttribute(double min, double max)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public sealed class SortAttribute : Attribute
	{
		public SortAttribute(int priority)
		{
		}
	}

	private float _addExp = 100f;

	private FacilityPlayerLevel _playerLevel;

	private MasterDataLoader _masterDataLoader;

	private DebugService _debugService;

	private AchievementService _achievementService;

	private StoryScenarioType _storyScenarioType = StoryScenarioType.MainScenario;

	private float _storyEpisodeNumber = 1f;

	private ShortTalkType _oneWordScenarioType = ShortTalkType.HeroineClickNormal;

	private int _oneWordEpisodeNumber = 1;

	private int _answerChoiceEpisodeNumber = 1;

	private HeroineAI.ActionStateType _actionStateType = HeroineAI.ActionStateType.WorkPC;

	private HeroineService.AnimationType _motionType;

	private LeaveChairJudge.LeaveChairDestination _debugLeaveChairDestination = LeaveChairJudge.LeaveChairDestination.Far;

	private LeaveChairKitchenSoundKind _debugLeaveChairKitchenSound;

	private HeroineCupcake.CupcakeKind _cupcakeKind;

	private int _pomodoroRemainingMinutes = 3;

	private float _pomodoroMovedSeconds;

	private bool _isDebugPomodoroVoiceEnabled;

	private bool _isDebugPomodoroVoicePlayEnabled;

	private bool _isDebugPomodoroLongWorkStartVoicePlayEnabled;

	private bool _isDebugPomodoroContinuousWorkVoicePlayEnabled;

	private bool _isDebugPomodoroShortWorkedBreakStartVoicePlayEnabled;

	private bool _isDebugPomodoroLongWorkedBreakStartVoicePlayEnabled;

	private bool _isDebugPomodoroLongWorkFinishVoicePlayEnabled;

	private bool _isDebugPomodoroShortWorkFinishVoicePlayEnabled;

	private bool _isDebugPomodoroMidwayFinishVoicePlayEnabled;

	private int _pomodoroWorkVoiceEpisodeNumber = 1;

	private int _pomodoroBreakVoiceEpisodeNumber = 1;

	private int _pomodoroFinishVoiceEpisodeNumber = 1;

	private bool _isDebugPomodoroMotionEnabled;

	private HeroineAI.ActionStateType _pomodoroWorkStartMotionType = HeroineAI.ActionStateType.WildStretchFullBody;

	private HeroineAI.ActionStateType _pomodoroBreakMotionType = HeroineAI.ActionStateType.WildStretchFullBody;

	private HeroineAI.ActionStateType _pomodoroFinishMotionType = HeroineAI.ActionStateType.WildStretchFullBody;

	private bool _isUsePhotographyDebugEnabled;

	private float _photographyStartTime;

	private string _scenarioCheckStartLabel = string.Empty;

	private GameStartDirectionScenarioType _gameStartDirectionScenarioType;

	private int _gameStartDirectionEpisodeNumber = 1;

	private GameEndDirectionScenarioType _gameEndDirectionScenarioType = GameEndDirectionScenarioType.GameEnd;

	private int _gameEndDirectionEpisodeNumber = 1;

	private bool _isAdjustAllVolume;

	private AchievementCategory _achievementCategory;

	private bool ondemandRendering;

	private RoomCameraManager.VirtualCameraType targetCamera;

	public static bool IsPurchasedStandardEditionDebug = true;

	public static bool EnablePurchasedStandardEditionDebug = false;

	private int overrideReadNewsId;

	private static SROptions _current;

	public static SROptions Current => _current;

	public event SROptionsPropertyChanged PropertyChanged;

	private event PropertyChangedEventHandler InterfacePropertyChangedEventHandler;

	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			InterfacePropertyChangedEventHandler += value;
		}
		remove
		{
			InterfacePropertyChangedEventHandler -= value;
		}
	}

	public void OnPropertyChanged(string propertyName)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, propertyName);
		}
		if (this.InterfacePropertyChangedEventHandler != null)
		{
			this.InterfacePropertyChangedEventHandler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
