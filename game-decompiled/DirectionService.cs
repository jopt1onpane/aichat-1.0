using Bulbul;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VContainer;

public class DirectionService : MonoBehaviour
{
	public enum RenderFeatureType
	{
		ColorGlitch,
		ScreenMove,
		Glitch,
		Blackout,
		SimpleNoise,
		BlackOutAdditive
	}

	private string ColorGlitchRenderFeatureName = "ColorGlitch";

	private string ScreenMoveRenderFeatureName = "ScreenMove";

	private string GlitchRenderFeatureName = "Glitch";

	private string BlackoutRenderFeatureName = "Blackout";

	private string SimpleNoiseRenderFeatureName = "SimpleNoise";

	private string BlackOutAdditiveRenderFeatureName = "BlackOutAdditive";

	[Inject]
	private PlayerLevelService _playerLevelService;

	[Inject]
	private SlideFadeAnnounceDirection _slideFadeAnnounce;

	[SerializeField]
	private UniversalRendererData _universalRendererDataLow;

	[SerializeField]
	private UniversalRendererData _universalRendererDataMiddle;

	[SerializeField]
	private UniversalRendererData _universalRendererDataHigh;

	[SerializeField]
	[Header("グリッチ")]
	private GlitchDirection _glitch;

	[SerializeField]
	[Header("画面ズレ")]
	private ScreenMoveDirection _screenMove;

	[SerializeField]
	[Header("ブラックアウト")]
	private BlackOutDirection _blackOut;

	[SerializeField]
	[Header("モノクロノイズ")]
	private MonochromeNoiseDirection _monochromeNoise;

	[SerializeField]
	[Header("色収差")]
	private ChromaticAberrationDirection _chromaticAberration;

	[SerializeField]
	[Header("画面共有")]
	private ScreenSharingDirection _screenSharing;

	[SerializeField]
	[Header("カメラ停止")]
	private CameraStopDirection _cameraStop;

	[SerializeField]
	[Header("全不具合値調整")]
	private AdjustAllUnstableDirection _adjustAllUnstable;

	[SerializeField]
	[Header("開始カメラ演出")]
	private GameStartCameraDirection _gameStartCameraDirection;

	[SerializeField]
	[Header("ゲーム中不具合演出")]
	private GamePlayingDefectDirection _gamePlayingDefectDirection;

	[SerializeField]
	[Header("31話読了後用切断演出")]
	private GlitchNoImageDirection _glitchNoImageDirection;

	private float _playDelaySecondsForStory;

	private bool _isActiveColorGlitchRenderFeature;

	private bool _isActiveScreenMoveRenderFeature;

	private bool _isActiveGlitchRenderFeature;

	private bool _isActiveBlackoutRenderFeature;

	private bool _isActiveSimpleNoiseRenderFeature;

	private bool _isActiveBlackOutAdditiveRenderFeature;

	public GlitchDirection Glitch => _glitch;

	public ScreenMoveDirection ScreenMove => _screenMove;

	public BlackOutDirection BlackOut => _blackOut;

	public MonochromeNoiseDirection MonochromeNoise => _monochromeNoise;

	public ChromaticAberrationDirection ChromaticAberration => _chromaticAberration;

	public ScreenSharingDirection ScreenSharing => _screenSharing;

	public SlideFadeAnnounceDirection SlideFadeAnnounce => _slideFadeAnnounce;

	public CameraStopDirection CameraStop => _cameraStop;

	public AdjustAllUnstableDirection AdjustAllUnstable => _adjustAllUnstable;

	public GameStartCameraDirection GameStartCameraDirection => _gameStartCameraDirection;

	public GamePlayingDefectDirection GamePlayingDefect => _gamePlayingDefectDirection;

	public GlitchNoImageDirection GlitchNoImageDirection => _glitchNoImageDirection;

	public float PlayDelaySecondsForStory => _playDelaySecondsForStory;

	public void Setup()
	{
		ChangeActiveRenderFeature(RenderFeatureType.ColorGlitch, toActive: false, isForce: true);
		ChangeActiveRenderFeature(RenderFeatureType.ScreenMove, toActive: false, isForce: true);
		ChangeActiveRenderFeature(RenderFeatureType.Glitch, toActive: false, isForce: true);
		ChangeActiveRenderFeature(RenderFeatureType.Blackout, toActive: false, isForce: true);
		ChangeActiveRenderFeature(RenderFeatureType.SimpleNoise, toActive: false, isForce: true);
		ChangeActiveRenderFeature(RenderFeatureType.BlackOutAdditive, toActive: false, isForce: true);
		_slideFadeAnnounce.InitSetup(_playerLevelService);
		_glitch.Setup(this);
		_screenMove.Setup(this);
		_blackOut.Setup(this);
		_monochromeNoise.Setup(this);
		_chromaticAberration.Setup(this);
		_glitchNoImageDirection.Setup(this);
		_adjustAllUnstable.Setup();
		_cameraStop.Setup();
		_screenSharing.Setup();
		_gameStartCameraDirection.Setup();
		_gamePlayingDefectDirection.Setup();
	}

	public void EndTidying()
	{
		_glitch.EndTidying();
		_screenMove.EndTidying();
		_blackOut.EndTidying();
		_monochromeNoise.EndTidying();
		_chromaticAberration.EndTidying();
		_adjustAllUnstable.EndTidying();
		_cameraStop.EndTidying();
		_screenSharing.EndTidying();
		_gameStartCameraDirection.EndTidying();
		_glitchNoImageDirection.EndTidying();
		_playDelaySecondsForStory = 0f;
	}

	public void SetPlayDelaySeconds(float playDelaySeconds)
	{
		_playDelaySecondsForStory = playDelaySeconds;
	}

	public void OnUsePlayDelaySeconds()
	{
		_playDelaySecondsForStory = 0f;
	}

	public void ChangeActiveRenderFeature(RenderFeatureType type, bool toActive, bool isForce = false)
	{
		string text = string.Empty;
		switch (type)
		{
		case RenderFeatureType.ColorGlitch:
			if (_isActiveColorGlitchRenderFeature == toActive && !isForce)
			{
				return;
			}
			text = ColorGlitchRenderFeatureName;
			_isActiveColorGlitchRenderFeature = toActive;
			break;
		case RenderFeatureType.ScreenMove:
			if (_isActiveScreenMoveRenderFeature == toActive && !isForce)
			{
				return;
			}
			text = ScreenMoveRenderFeatureName;
			_isActiveScreenMoveRenderFeature = toActive;
			break;
		case RenderFeatureType.Glitch:
			if (_isActiveGlitchRenderFeature == toActive && !isForce)
			{
				return;
			}
			text = GlitchRenderFeatureName;
			_isActiveGlitchRenderFeature = toActive;
			break;
		case RenderFeatureType.Blackout:
			if (_isActiveBlackoutRenderFeature == toActive && !isForce)
			{
				return;
			}
			text = BlackoutRenderFeatureName;
			_isActiveBlackoutRenderFeature = toActive;
			break;
		case RenderFeatureType.SimpleNoise:
			if (_isActiveSimpleNoiseRenderFeature == toActive && !isForce)
			{
				return;
			}
			text = SimpleNoiseRenderFeatureName;
			_isActiveSimpleNoiseRenderFeature = toActive;
			break;
		case RenderFeatureType.BlackOutAdditive:
			if (_isActiveBlackOutAdditiveRenderFeature == toActive && !isForce)
			{
				return;
			}
			text = BlackOutAdditiveRenderFeatureName;
			_isActiveBlackOutAdditiveRenderFeature = toActive;
			break;
		}
		foreach (ScriptableRendererFeature rendererFeature in _universalRendererDataLow.rendererFeatures)
		{
			if (rendererFeature != null && rendererFeature.name == text)
			{
				rendererFeature.SetActive(toActive);
				break;
			}
		}
		foreach (ScriptableRendererFeature rendererFeature2 in _universalRendererDataMiddle.rendererFeatures)
		{
			if (rendererFeature2 != null && rendererFeature2.name == text)
			{
				rendererFeature2.SetActive(toActive);
				break;
			}
		}
		foreach (ScriptableRendererFeature rendererFeature3 in _universalRendererDataHigh.rendererFeatures)
		{
			if (rendererFeature3 != null && rendererFeature3.name == text)
			{
				rendererFeature3.SetActive(toActive);
				break;
			}
		}
	}
}
