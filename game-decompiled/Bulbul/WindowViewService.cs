using System;
using FastEnumUtility;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class WindowViewService : MonoBehaviour
{
	[Serializable]
	public struct PlatformSwitchWindowViewData
	{
		public MeshRenderer _windowViewMeshRenderer;

		public Material _mobileMaterial;

		public void SwitchMobile()
		{
			_windowViewMeshRenderer.sharedMaterial = _mobileMaterial;
		}
	}

	[Inject]
	private MyCozyWeatherService _cozyWeatherService;

	[Header("時間、天候オブジェクト")]
	[SerializeField]
	private GameObject _day;

	[SerializeField]
	private GameObject _sunset;

	[SerializeField]
	private GameObject _night;

	[SerializeField]
	private GameObject _cloudy;

	[Header("窓の外オブジェクト")]
	[SerializeField]
	private GameObject _fireworks;

	[SerializeField]
	private GameObject _deepSea;

	[SerializeField]
	private GameObject _books;

	[SerializeField]
	private GameObject _windBell;

	[SerializeField]
	private GameObject _sakura;

	[SerializeField]
	private GameObject _snow;

	[SerializeField]
	private GameObject _jet;

	[SerializeField]
	private GameObject _balloon;

	[SerializeField]
	private GameObject _lightRain;

	[SerializeField]
	private GameObject _heavyRain;

	[SerializeField]
	private GameObject _thunderRain;

	[SerializeField]
	private GameObject _whale;

	[SerializeField]
	private GameObject _hotSpring;

	[SerializeField]
	private GameObject _space;

	[SerializeField]
	private GameObject _locomotive;

	[SerializeField]
	private GameObject _blueButterfly;

	[SerializeField]
	private GameObject _city;

	[SerializeField]
	private GameObject _jellyFish;

	[SerializeField]
	private GameObject _valentineSweets;

	[SerializeField]
	private GameObject _aurora;

	[SerializeField]
	[Header("プラットフォーム用のマテリアルに切り替える\u3000初期設定はPCのものにしておく")]
	private PlatformSwitchWindowViewData[] _platformSwitchViewData;

	private WindowViewType _currentTimeOfDayType;

	public void ChangeWeatherAndTime(WindowViewType weatherAndTimeType)
	{
		_currentTimeOfDayType = weatherAndTimeType;
		_day.gameObject.SetActive(weatherAndTimeType == WindowViewType.Day);
		_sunset.gameObject.SetActive(weatherAndTimeType == WindowViewType.Sunset);
		_night.gameObject.SetActive(weatherAndTimeType == WindowViewType.Night);
		_cloudy.gameObject.SetActive(weatherAndTimeType == WindowViewType.Cloudy);
		_cozyWeatherService.ChangeCozyWeather(weatherAndTimeType);
	}

	public void ChangeWindowView(WindowViewType viewType)
	{
		switch (viewType)
		{
		case WindowViewType.Fireworks:
			_fireworks.gameObject.SetActive(viewType == WindowViewType.Fireworks);
			break;
		case WindowViewType.DeepSea:
			_deepSea.gameObject.SetActive(viewType == WindowViewType.DeepSea);
			break;
		case WindowViewType.Books:
			_books.gameObject.SetActive(viewType == WindowViewType.Books);
			break;
		case WindowViewType.WindBell:
			_windBell.gameObject.SetActive(viewType == WindowViewType.WindBell);
			break;
		case WindowViewType.Sakura:
			_sakura.gameObject.SetActive(viewType == WindowViewType.Sakura);
			break;
		case WindowViewType.Snow:
			_snow.gameObject.SetActive(viewType == WindowViewType.Snow);
			break;
		case WindowViewType.Jet:
			_jet.gameObject.SetActive(viewType == WindowViewType.Jet);
			break;
		case WindowViewType.Balloon:
			_balloon.gameObject.SetActive(viewType == WindowViewType.Balloon);
			break;
		case WindowViewType.LightRain:
			_lightRain.gameObject.SetActive(viewType == WindowViewType.LightRain);
			break;
		case WindowViewType.HeavyRain:
			_heavyRain.gameObject.SetActive(viewType == WindowViewType.HeavyRain);
			break;
		case WindowViewType.ThunderRain:
			_thunderRain.gameObject.SetActive(viewType == WindowViewType.ThunderRain);
			break;
		case WindowViewType.Whale:
			_whale.gameObject.SetActive(viewType == WindowViewType.Whale);
			break;
		case WindowViewType.HotSpring:
			_hotSpring.gameObject.SetActive(viewType == WindowViewType.HotSpring);
			break;
		case WindowViewType.Space:
			_space.gameObject.SetActive(viewType == WindowViewType.Space);
			break;
		case WindowViewType.Locomotive:
			_locomotive.gameObject.SetActive(viewType == WindowViewType.Locomotive);
			break;
		case WindowViewType.Day:
		case WindowViewType.Sunset:
		case WindowViewType.Night:
		case WindowViewType.Cloudy:
			ChangeWeatherAndTime(viewType);
			break;
		}
	}

	public void ActivateWindow(WindowViewType viewType)
	{
		switch (viewType)
		{
		case WindowViewType.Fireworks:
			_fireworks.gameObject.SetActive(value: true);
			break;
		case WindowViewType.DeepSea:
			_deepSea.gameObject.SetActive(value: true);
			break;
		case WindowViewType.Books:
			_books.gameObject.SetActive(value: true);
			break;
		case WindowViewType.WindBell:
			_windBell.gameObject.SetActive(value: true);
			break;
		case WindowViewType.Sakura:
			_sakura.gameObject.SetActive(value: true);
			break;
		case WindowViewType.Snow:
			_snow.gameObject.SetActive(value: true);
			break;
		case WindowViewType.Jet:
			_jet.gameObject.SetActive(value: true);
			break;
		case WindowViewType.Balloon:
			_balloon.gameObject.SetActive(value: true);
			break;
		case WindowViewType.LightRain:
			_lightRain.gameObject.SetActive(value: true);
			break;
		case WindowViewType.HeavyRain:
			_heavyRain.gameObject.SetActive(value: true);
			break;
		case WindowViewType.ThunderRain:
			_thunderRain.gameObject.SetActive(value: true);
			break;
		case WindowViewType.Whale:
			_whale.gameObject.SetActive(value: true);
			break;
		case WindowViewType.HotSpring:
			_hotSpring.gameObject.SetActive(value: true);
			break;
		case WindowViewType.Space:
			_space.gameObject.SetActive(value: true);
			break;
		case WindowViewType.Locomotive:
			_locomotive.gameObject.SetActive(value: true);
			break;
		case WindowViewType.BlueButterfly:
			_blueButterfly.gameObject.SetActive(value: true);
			break;
		case WindowViewType.City:
			_city.gameObject.SetActive(value: true);
			break;
		case WindowViewType.Jellyfish:
			_jellyFish.gameObject.SetActive(value: true);
			break;
		case WindowViewType.ValentineSweets:
			_valentineSweets.gameObject.SetActive(value: true);
			break;
		case WindowViewType.Aurora:
			_aurora.gameObject.SetActive(value: true);
			break;
		case WindowViewType.Day:
		case WindowViewType.Sunset:
		case WindowViewType.Night:
		case WindowViewType.Cloudy:
			ChangeWeatherAndTime(viewType);
			break;
		}
	}

	public void DeactivateWindow(WindowViewType viewType)
	{
		switch (viewType)
		{
		case WindowViewType.Fireworks:
			_fireworks.gameObject.SetActive(value: false);
			break;
		case WindowViewType.DeepSea:
			_deepSea.gameObject.SetActive(value: false);
			break;
		case WindowViewType.Books:
			_books.gameObject.SetActive(value: false);
			break;
		case WindowViewType.WindBell:
			_windBell.gameObject.SetActive(value: false);
			break;
		case WindowViewType.Sakura:
			_sakura.gameObject.SetActive(value: false);
			break;
		case WindowViewType.Snow:
			_snow.gameObject.SetActive(value: false);
			break;
		case WindowViewType.Jet:
			_jet.gameObject.SetActive(value: false);
			break;
		case WindowViewType.Balloon:
			_balloon.gameObject.SetActive(value: false);
			break;
		case WindowViewType.LightRain:
			_lightRain.gameObject.SetActive(value: false);
			break;
		case WindowViewType.HeavyRain:
			_heavyRain.gameObject.SetActive(value: false);
			break;
		case WindowViewType.ThunderRain:
			_thunderRain.gameObject.SetActive(value: false);
			break;
		case WindowViewType.Whale:
			_whale.gameObject.SetActive(value: false);
			break;
		case WindowViewType.HotSpring:
			_hotSpring.gameObject.SetActive(value: false);
			break;
		case WindowViewType.Space:
			_space.gameObject.SetActive(value: false);
			break;
		case WindowViewType.Locomotive:
			_locomotive.gameObject.SetActive(value: false);
			break;
		case WindowViewType.BlueButterfly:
			_blueButterfly.gameObject.SetActive(value: false);
			break;
		case WindowViewType.City:
			_city.gameObject.SetActive(value: false);
			break;
		case WindowViewType.Jellyfish:
			_jellyFish.gameObject.SetActive(value: false);
			break;
		case WindowViewType.ValentineSweets:
			_valentineSweets.gameObject.SetActive(value: false);
			break;
		case WindowViewType.Aurora:
			_aurora.gameObject.SetActive(value: false);
			break;
		case WindowViewType.Day:
		case WindowViewType.Sunset:
		case WindowViewType.Night:
		case WindowViewType.Cloudy:
			break;
		}
	}

	public bool IsActiveWindow(WindowViewType viewType)
	{
		bool result = false;
		switch (viewType)
		{
		case WindowViewType.Day:
			result = _day.gameObject.activeSelf;
			break;
		case WindowViewType.Sunset:
			result = _sunset.gameObject.activeSelf;
			break;
		case WindowViewType.Night:
			result = _night.gameObject.activeSelf;
			break;
		case WindowViewType.Cloudy:
			result = _cloudy.gameObject.activeSelf;
			break;
		case WindowViewType.Fireworks:
			result = _fireworks.gameObject.activeSelf;
			break;
		case WindowViewType.DeepSea:
			result = _deepSea.gameObject.activeSelf;
			break;
		case WindowViewType.Books:
			result = _books.gameObject.activeSelf;
			break;
		case WindowViewType.WindBell:
			result = _windBell.gameObject.activeSelf;
			break;
		case WindowViewType.Sakura:
			result = _sakura.gameObject.activeSelf;
			break;
		case WindowViewType.Snow:
			result = _snow.gameObject.activeSelf;
			break;
		case WindowViewType.Jet:
			result = _jet.gameObject.activeSelf;
			break;
		case WindowViewType.Balloon:
			result = _balloon.gameObject.activeSelf;
			break;
		case WindowViewType.LightRain:
			result = _lightRain.gameObject.activeSelf;
			break;
		case WindowViewType.HeavyRain:
			result = _heavyRain.gameObject.activeSelf;
			break;
		case WindowViewType.ThunderRain:
			result = _thunderRain.gameObject.activeSelf;
			break;
		case WindowViewType.Whale:
			result = _whale.gameObject.activeSelf;
			break;
		case WindowViewType.HotSpring:
			result = _hotSpring.gameObject.activeSelf;
			break;
		case WindowViewType.Space:
			result = _space.gameObject.activeSelf;
			break;
		case WindowViewType.Locomotive:
			result = _locomotive.gameObject.activeSelf;
			break;
		case WindowViewType.BlueButterfly:
			result = _blueButterfly.gameObject.activeSelf;
			break;
		case WindowViewType.City:
			result = _city.gameObject.activeSelf;
			break;
		case WindowViewType.Jellyfish:
			result = _jellyFish.gameObject.activeSelf;
			break;
		case WindowViewType.ValentineSweets:
			result = _valentineSweets.gameObject.activeSelf;
			break;
		case WindowViewType.Aurora:
			result = _aurora.gameObject.activeSelf;
			break;
		default:
			Debug.LogError($"{viewType}が定義されていません。");
			break;
		}
		return result;
	}

	public void ResetAll()
	{
		foreach (WindowViewType value in FastEnum.GetValues<WindowViewType>())
		{
			if (value == WindowViewType.Day)
			{
				ActivateWindow(value);
			}
			else
			{
				DeactivateWindow(value);
			}
		}
	}

	public void ResetOtherThanTimeOfDay()
	{
		foreach (WindowViewType value in FastEnum.GetValues<WindowViewType>())
		{
			if (value != WindowViewType.Day && value != WindowViewType.Sunset && value != WindowViewType.Night && value != WindowViewType.Cloudy)
			{
				DeactivateWindow(value);
			}
		}
	}
}
