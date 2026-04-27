using Bulbul;
using R3;
using UnityEngine;
using VContainer;

public class NoiseController : MonoBehaviour
{
	[Inject]
	private DirectionService _directionService;

	[Inject]
	private AmbientSeService _ambientSeService;

	[SerializeField]
	[Range(0f, 10f)]
	[Header("ノイズオーディオ\nグリッチ不具合音声反映率")]
	private float _glitchNoiseVolumeInfluenceRatio;

	[SerializeField]
	[Range(0f, 10f)]
	[Header("モノクロノイズ不具合音声反映率")]
	private float _monochromeNoiseVolumeInfluenceRatio;

	[SerializeField]
	[Range(0f, 10f)]
	[Header("画面ズレ不具合音声反映率")]
	private float _screenMoveNoiseVolumeInfluenceRatio;

	[SerializeField]
	[Range(0f, 10f)]
	[Header("ブラックアウト不具合音声反映率")]
	private float _blackOutNoiseVolumeInfluenceRatio;

	private void Awake()
	{
		Setup();
	}

	public void Setup()
	{
		_directionService.Glitch.PowerRatio.Subscribe(delegate(float ratio)
		{
			if (_directionService.GamePlayingDefect.IsAlwaysNoise())
			{
				StopGlitchNoiseSE();
			}
			else if (ratio == 0f)
			{
				StopGlitchNoiseSE();
			}
			else if (ratio != 0f)
			{
				float volumeRate = ratio * _glitchNoiseVolumeInfluenceRatio;
				PlayGlitchNoiseSE(volumeRate);
			}
		}).AddTo(this);
		_directionService.MonochromeNoise.PowerRatio.Subscribe(delegate(float ratio)
		{
			if (_directionService.GamePlayingDefect.IsAlwaysNoise())
			{
				StopMonochoromeNoiseSE();
			}
			else if (ratio == 0f)
			{
				StopMonochoromeNoiseSE();
			}
			else if (ratio != 0f)
			{
				float volumeRate = ratio * _monochromeNoiseVolumeInfluenceRatio;
				PlayMonochoromeNoiseSE(volumeRate);
			}
		}).AddTo(this);
		_directionService.ScreenMove.PowerRatio.Subscribe(delegate(float ratio)
		{
			if (_directionService.GamePlayingDefect.IsAlwaysNoise())
			{
				StopScreenMoveSE();
			}
			else if (ratio == 0f)
			{
				StopScreenMoveSE();
			}
			else if (ratio != 0f)
			{
				float volumeRate = ratio * _screenMoveNoiseVolumeInfluenceRatio;
				PlayScreenMoveSE(volumeRate);
			}
		}).AddTo(this);
		_directionService.BlackOut.PowerRatio.Subscribe(delegate(float ratio)
		{
			if (_directionService.GamePlayingDefect.IsAlwaysNoise())
			{
				StopBlackOutSE();
			}
			else if (ratio == 0f)
			{
				StopBlackOutSE();
			}
			else if (ratio != 0f)
			{
				float volumeRate = ratio * _blackOutNoiseVolumeInfluenceRatio;
				PlayBlackOutSE(volumeRate);
			}
		}).AddTo(this);
	}

	public void ActivateGamePlayingDefectAlwaysNoise()
	{
		StopGlitchNoiseSE();
		StopMonochoromeNoiseSE();
		StopScreenMoveSE();
		StopBlackOutSE();
	}

	public void PlayGlitchNoiseSE(float volumeRate = 1f)
	{
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = AmbientSeType.GlitchNoise,
			IsAllowsDuplicate = false,
			VolumeRate = volumeRate
		});
	}

	public void StopGlitchNoiseSE()
	{
		_ambientSeService.Stop(AmbientSeType.GlitchNoise);
	}

	public void PlayMonochoromeNoiseSE(float volumeRate = 1f)
	{
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = AmbientSeType.MonochoromeNoise,
			IsAllowsDuplicate = false,
			VolumeRate = volumeRate
		});
	}

	public void StopMonochoromeNoiseSE()
	{
		_ambientSeService.Stop(AmbientSeType.MonochoromeNoise);
	}

	public void PlayScreenMoveSE(float volumeRate = 1f)
	{
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = AmbientSeType.ScreenMove,
			IsAllowsDuplicate = false,
			VolumeRate = volumeRate
		});
	}

	public void StopScreenMoveSE()
	{
		_ambientSeService.Stop(AmbientSeType.ScreenMove);
	}

	public void PlayBlackOutSE(float volumeRate = 1f)
	{
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = AmbientSeType.BlackOut,
			IsAllowsDuplicate = false,
			VolumeRate = volumeRate
		});
	}

	public void StopBlackOutSE()
	{
		_ambientSeService.Stop(AmbientSeType.BlackOut);
	}
}
