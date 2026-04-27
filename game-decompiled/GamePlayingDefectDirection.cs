using Bulbul;
using R3;
using UnityEngine;
using VContainer;

public class GamePlayingDefectDirection : MonoBehaviour
{
	public enum DefectType
	{
		None,
		AlwaysNoise,
		ConnectionLost
	}

	private DefectType _currentDefectType;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private AudioMixerGroupContainer _audioMixerContainer;

	[Inject]
	private IDirectionServiceUIProvider _directionServiceUIProvider;

	[SerializeField]
	private NoiseController _noiseController;

	private GameObject _connectionLostGameObj;

	private ReactiveProperty<bool> _isUseConnectionLost = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsUseConnectionLost => _isUseConnectionLost;

	public void Setup()
	{
		_connectionLostGameObj = _directionServiceUIProvider.ConnectionLost;
		Init();
	}

	public void Init()
	{
		_isUseConnectionLost.Value = false;
		_directionService.EndTidying();
		InitDefectAudioEffect();
		Camera.main.enabled = true;
		_connectionLostGameObj.SetActive(value: false);
		_currentDefectType = DefectType.None;
	}

	public bool IsUsing()
	{
		return _currentDefectType != DefectType.None;
	}

	public bool IsAlwaysNoise()
	{
		return _currentDefectType == DefectType.AlwaysNoise;
	}

	public bool IsConnectionLost()
	{
		return _currentDefectType == DefectType.ConnectionLost;
	}

	public bool IsUseDefectForEpisodeDirection()
	{
		if (SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber == 30f || SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber == 31f)
		{
			return true;
		}
		return false;
	}

	public bool IsNextLevelConnectionLost()
	{
		return SaveDataManager.Instance.PlayerData.LevelData.CurrentLevel == 31;
	}

	public bool GetLevelUpperForConnectionLost()
	{
		return SaveDataManager.Instance.PlayerData.LevelData.CurrentLevel == 32;
	}

	public bool IsPossibleReconnect()
	{
		return SaveDataManager.Instance.PlayerData.LevelData.CurrentLevel >= 33;
	}

	public DefectType CheckNeedUseDefectDirection()
	{
		if (SaveDataManager.Instance.ScenarioProgressData.NextEpisodeNumber == 31f)
		{
			return DefectType.AlwaysNoise;
		}
		if (SaveDataManager.Instance.ScenarioProgressData.NextEpisodeNumber == 32f)
		{
			return DefectType.ConnectionLost;
		}
		return DefectType.None;
	}

	public void PlayDefectDirection(DefectType defectType)
	{
		Init();
		switch (defectType)
		{
		case DefectType.AlwaysNoise:
			UseAlwaysNoise();
			break;
		case DefectType.ConnectionLost:
			UseConnectionLost();
			break;
		}
		_currentDefectType = defectType;
	}

	private void UseAlwaysNoise()
	{
		_directionService.AdjustAllUnstable.Play(0.17f, 0f);
		_directionService.BlackOut.Play(0f, 0);
		_directionService.ChromaticAberration.Play(0f, 0f);
		_directionService.ScreenMove.Play(0f);
		_directionService.MonochromeNoise.Play(0.15f, 0f);
		_noiseController.ActivateGamePlayingDefectAlwaysNoise();
		UseDefectAudioEffect();
	}

	private void UseConnectionLost()
	{
		_isUseConnectionLost.Value = true;
		_directionService.GlitchNoImageDirection.PlayGlitch();
		_connectionLostGameObj.SetActive(value: true);
		UseDefectAudioEffect();
	}

	private void InitDefectAudioEffect()
	{
		_audioMixerContainer.SEGroup.audioMixer.SetFloat("SELowpassFreq", 22000f);
		_audioMixerContainer.AmbientBGMGroup.audioMixer.SetFloat("AmbientBGMLowpassFreq", 22000f);
		_audioMixerContainer.AmbientSEGroup.audioMixer.SetFloat("AmbientSELowpassFreq", 22000f);
	}

	private void UseDefectAudioEffect()
	{
		_audioMixerContainer.SEGroup.audioMixer.SetFloat("SELowpassFreq", 2500f);
		_audioMixerContainer.AmbientBGMGroup.audioMixer.SetFloat("AmbientBGMLowpassFreq", 6200f);
		_audioMixerContainer.AmbientSEGroup.audioMixer.SetFloat("AmbientSELowpassFreq", 5000f);
	}
}
