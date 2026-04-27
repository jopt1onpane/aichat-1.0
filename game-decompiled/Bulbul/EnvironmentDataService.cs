using System;
using FastEnumUtility;
using R3;

namespace Bulbul;

public class EnvironmentDataService : IPresetDataService, IDisposable
{
	private readonly Subject<EnvironmentType> _onFirstPlayed = new Subject<EnvironmentType>();

	private SaveDataManager SaveData => SaveDataManager.Instance;

	public Observable<EnvironmentType> OnFirstPlayed => _onFirstPlayed;

	public bool IsWindowActive(WindowViewType windowViewType)
	{
		return GetWindowViewData(windowViewType).IsActive;
	}

	public void SetViewActive(WindowViewType windowViewType, bool isActive)
	{
		WindowViewData windowViewData = GetWindowViewData(windowViewType);
		if (windowViewData.IsActive != isActive)
		{
			windowViewData.IsActive = isActive;
			SaveData.SaveEnviromentThrottled();
		}
	}

	public bool IsMute(AmbientSoundType ambientSoundType)
	{
		return GetAmbientSoundData(ambientSoundType).IsMuteAmbient;
	}

	public bool IsMuteOrVolumeZero(AmbientSoundType ambientSoundType)
	{
		AmbientSoundData ambientSoundData = GetAmbientSoundData(ambientSoundType);
		if (!ambientSoundData.IsMuteAmbient)
		{
			return ambientSoundData.SoundVolume <= 0f;
		}
		return true;
	}

	public (float volume, bool isMute) GetVolume(AmbientSoundType ambientSoundType)
	{
		AmbientSoundData ambientSoundData = GetAmbientSoundData(ambientSoundType);
		return (volume: ambientSoundData.SoundVolume, isMute: ambientSoundData.IsMuteAmbient);
	}

	public void SetVolume(AmbientSoundType ambientSoundType, float volume)
	{
		AmbientSoundData ambientSoundData = GetAmbientSoundData(ambientSoundType);
		if (ambientSoundData.SoundVolume != volume)
		{
			ambientSoundData.SoundVolume = volume;
			SaveData.SaveEnviromentThrottled();
		}
	}

	public void SetMute(AmbientSoundType ambientSoundType, bool isMute)
	{
		AmbientSoundData ambientSoundData = GetAmbientSoundData(ambientSoundType);
		if (ambientSoundData.IsMuteAmbient != isMute)
		{
			ambientSoundData.IsMuteAmbient = isMute;
			SaveData.SaveEnviromentThrottled();
		}
	}

	public bool HavePlayed(EnvironmentType environmentType)
	{
		string environmentNameForPlayedList = GetEnvironmentNameForPlayedList(environmentType);
		return SaveData.EnvironmentProgressData.PlayedEnvironment.Contains(environmentNameForPlayedList);
	}

	public void SetPlayed(EnvironmentType environmentType)
	{
		string environmentNameForPlayedList = GetEnvironmentNameForPlayedList(environmentType);
		if (!SaveData.EnvironmentProgressData.PlayedEnvironment.Contains(environmentNameForPlayedList))
		{
			SaveData.EnvironmentProgressData.PlayedEnvironment.Add(environmentNameForPlayedList);
			SaveData.SaveEnvironmentProgressData();
			_onFirstPlayed.OnNext(environmentType);
		}
	}

	private string GetEnvironmentNameForPlayedList(EnvironmentType environmentType)
	{
		if (environmentType.GetEnvironmentControllerType() == EnvironmentControllerType.SoundOnly)
		{
			environmentType.TryConvertToAmbientSoundType(out var ambientSoundType);
			return ambientSoundType.ToName();
		}
		environmentType.TryConvertToWindowViewType(out var windowViewType);
		return windowViewType.ToName();
	}

	private WindowViewData GetWindowViewData(WindowViewType windowViewType)
	{
		if (!SaveData.EnviromentData.WindowViewDic.TryGetValue(windowViewType, out var value))
		{
			value = new WindowViewData(windowViewType);
			SaveData.EnviromentData.WindowViewDic[windowViewType] = value;
		}
		return value;
	}

	private AmbientSoundData GetAmbientSoundData(AmbientSoundType ambientSoundType)
	{
		if (!SaveData.EnviromentData.AmbientSoundDic.TryGetValue(ambientSoundType, out var value))
		{
			value = new AmbientSoundData(ambientSoundType);
			SaveData.EnviromentData.AmbientSoundDic[ambientSoundType] = value;
		}
		return value;
	}

	public void LoadPreset(int index)
	{
		EnviromentData orCreatePreset = GetOrCreatePreset(index);
		SaveData.EnviromentData.CopyFrom(orCreatePreset);
		SaveData.SaveEnviromentThrottled();
		if (SaveData.EnviromentPresetsData.SelectedIndex != index)
		{
			SaveData.EnviromentPresetsData.SelectedIndex = index;
			SaveData.SaveEnviromentPresetThrottled();
		}
	}

	public void SaveCurrentToPreset(int index, bool alsoSetSelectedIndex)
	{
		GetOrCreatePreset(index).CopyFrom(SaveData.EnviromentData);
		if (alsoSetSelectedIndex)
		{
			SaveData.EnviromentPresetsData.SelectedIndex = index;
		}
		SaveData.SaveEnviromentPresetThrottled();
	}

	public int GetCurrentPresetIndex()
	{
		return SaveData.EnviromentPresetsData.SelectedIndex;
	}

	public bool HasDifferenceFromPreset(int index)
	{
		return !SaveData.EnviromentData.IsSame(GetOrCreatePreset(index));
	}

	private EnviromentData GetOrCreatePreset(int index)
	{
		EnviromentData enviromentData = SaveData.EnviromentPresetsData.Presets[index];
		if (enviromentData == null)
		{
			enviromentData = new EnviromentData();
			SaveData.EnviromentPresetsData.Presets[index] = enviromentData;
		}
		return enviromentData;
	}

	public void Dispose()
	{
	}
}
