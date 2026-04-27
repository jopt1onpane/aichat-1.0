using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DistantLands.Cozy;
using DistantLands.Cozy.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Bulbul;

public class MyCozyWeatherService : MonoBehaviour
{
	[Serializable]
	public class WeatherPair
	{
		public WeatherType WeatherType;

		public WeatherProfile WeatherProf;
	}

	[Serializable]
	public class AtmospherePair
	{
		public WeatherType WeatherType;

		public AtmosphereProfile AtomosphereProf;
	}

	private CozyWeather _cozyWeather;

	[SerializeField]
	private CozyWeather _dayCozyWeather;

	[SerializeField]
	private CozyWeather _sunsetCozyWeather;

	[SerializeField]
	private CozyWeather _nightCozyWeather;

	[SerializeField]
	private CozyWeather _cloudyCozyWeather;

	[SerializeField]
	private CozyWeather _rainCozyWeather;

	[SerializeField]
	private WeatherPair[] _weatherProfiles;

	[SerializeField]
	private AtmospherePair[] _atmosphereProfiles;

	[SerializeField]
	private Transform[] _blocksWeather;

	[SerializeField]
	private Transform _cozyWeatherEffectPosition;

	[Header("モバイル用の代替オブジェクト")]
	[SerializeField]
	private GameObject _mobileDaySky;

	[SerializeField]
	private GameObject _mobileSunsetSky;

	[SerializeField]
	private GameObject _mobileNightSky;

	[SerializeField]
	private GameObject _mobileCloudySky;

	private const string BLOCKWEATHER_LAYER = "BlockWeather";

	public bool IsReady { get; private set; }

	public async UniTask Setup()
	{
		bool needsMobileSetting = DevicePlatform.Steam.IsMobile();
		await InitAsync();
		async UniTask InitAsync()
		{
			_cozyWeather = CozyWeather.instance;
			if (_cozyWeather == null)
			{
				Debug.LogError("Setup Scene by COZY Weather Editor");
			}
			else
			{
				await UniTask.NextFrame(PlayerLoopTiming.LastPostLateUpdate, this.GetCancellationTokenOnDestroy());
				ParticleSystem[] componentsInChildren = _cozyWeather.transform.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					ParticleSystem.CollisionModule collision = componentsInChildren[i].collision;
					collision.enabled = true;
					collision.lifetimeLoss = 1f;
					collision.collidesWith = 1 << LayerMask.NameToLayer("BlockWeather");
					Transform[] blocksWeather = _blocksWeather;
					foreach (Transform transform in blocksWeather)
					{
						collision.AddPlane(transform);
					}
				}
				if (_cozyWeatherEffectPosition != null)
				{
					_cozyWeather.lockToCamera = CozyWeather.LockToCameraStyle.DontLockToCamera;
					_cozyWeather.centerAroundCustomObject = true;
					_cozyWeather.customPivot = _cozyWeatherEffectPosition;
				}
				Volume[] componentsInChildren2 = _cozyWeather.transform.GetComponentsInChildren<Volume>(includeInactive: true);
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					UnityEngine.Object.Destroy(componentsInChildren2[i].gameObject);
				}
				AudioSource[] componentsInChildren3 = _cozyWeather.transform.GetComponentsInChildren<AudioSource>(includeInactive: true);
				foreach (AudioSource obj in componentsInChildren3)
				{
					obj.clip = null;
					obj.outputAudioMixerGroup = null;
					obj.mute = true;
					obj.enabled = false;
					obj.gameObject.SetActive(value: false);
				}
				Light[] componentsInChildren4 = _cozyWeather.transform.GetComponentsInChildren<Light>(includeInactive: true);
				foreach (Light obj2 in componentsInChildren4)
				{
					obj2.cullingMask = 0;
					obj2.intensity = 0f;
					obj2.enabled = false;
					obj2.gameObject.SetActive(value: false);
				}
				_mobileDaySky.SetActive(needsMobileSetting);
				_mobileSunsetSky.SetActive(needsMobileSetting);
				_mobileNightSky.SetActive(needsMobileSetting);
				_mobileCloudySky.SetActive(needsMobileSetting);
				IsReady = true;
			}
		}
	}

	public void ChangeCozyWeather(WindowViewType wetherAndTimeType)
	{
		if (!IsPossibleUseCozyWeather() || !IsReady)
		{
			return;
		}
		bool flag = true;
		switch (wetherAndTimeType)
		{
		case WindowViewType.Day:
			_cozyWeather.timeModule = _dayCozyWeather.timeModule;
			_cozyWeather.climateModule = _dayCozyWeather.climateModule;
			_cozyWeather.windModule = _dayCozyWeather.windModule;
			_cozyWeather.interactionsModule = _dayCozyWeather.interactionsModule;
			ChangeAtmosphere(WeatherType.Day);
			if (flag)
			{
				ChangeWeather(WeatherType.Day);
			}
			break;
		case WindowViewType.Sunset:
			_cozyWeather.timeModule = _sunsetCozyWeather.timeModule;
			_cozyWeather.climateModule = _sunsetCozyWeather.climateModule;
			_cozyWeather.windModule = _sunsetCozyWeather.windModule;
			_cozyWeather.interactionsModule = _sunsetCozyWeather.interactionsModule;
			ChangeAtmosphere(WeatherType.Sunset);
			if (flag)
			{
				ChangeWeather(WeatherType.Sunset);
			}
			break;
		case WindowViewType.Night:
			_cozyWeather.timeModule = _nightCozyWeather.timeModule;
			_cozyWeather.climateModule = _nightCozyWeather.climateModule;
			_cozyWeather.windModule = _nightCozyWeather.windModule;
			_cozyWeather.interactionsModule = _nightCozyWeather.interactionsModule;
			ChangeAtmosphere(WeatherType.Night);
			if (flag)
			{
				ChangeWeather(WeatherType.Night);
			}
			break;
		case WindowViewType.Cloudy:
			_cozyWeather.timeModule = _cloudyCozyWeather.timeModule;
			_cozyWeather.climateModule = _cloudyCozyWeather.climateModule;
			_cozyWeather.windModule = _cloudyCozyWeather.windModule;
			_cozyWeather.interactionsModule = _cloudyCozyWeather.interactionsModule;
			ChangeAtmosphere(WeatherType.Cloudy);
			if (flag)
			{
				ChangeWeather(WeatherType.Cloudy);
			}
			break;
		}
	}

	public void ChangeWeather(WeatherType weather)
	{
		if (IsPossibleUseCozyWeather())
		{
			_cozyWeather.weatherModule.ecosystem.weatherSelectionMode = CozyEcosystem.EcosystemStyle.manual;
			_cozyWeather.weatherModule.ecosystem.currentWeather.SetWeatherWeight(0f);
			WeatherPair weatherPair = _weatherProfiles.FirstOrDefault((WeatherPair w) => w.WeatherType == weather);
			if (weatherPair == null || weatherPair.WeatherProf == null)
			{
				Debug.LogError($"WeatherType:{weather} profile is not set");
			}
			else
			{
				_cozyWeather.weatherModule.ecosystem.SetWeather(weatherPair.WeatherProf, 0f);
			}
		}
	}

	public void ChangeAtmosphere(WeatherType weather)
	{
		if (IsPossibleUseCozyWeather())
		{
			AtmospherePair atmospherePair = _atmosphereProfiles.FirstOrDefault((AtmospherePair w) => w.WeatherType == weather);
			if (atmospherePair == null || atmospherePair.AtomosphereProf == null)
			{
				Debug.LogError($"WeatherType:{weather} profile is not set");
			}
			else
			{
				_cozyWeather.atmosphereModule.atmosphereProfile = atmospherePair.AtomosphereProf;
			}
		}
	}

	public bool IsPossibleUseCozyWeather()
	{
		if (_cozyWeather == null)
		{
			return false;
		}
		return true;
	}
}
