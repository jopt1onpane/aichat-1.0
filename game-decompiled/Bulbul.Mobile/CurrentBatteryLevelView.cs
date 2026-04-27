using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class CurrentBatteryLevelView : MonoBehaviour
{
	private static readonly int _redBatteryLevelPer = 20;

	private static readonly string _format = "{0}%";

	[SerializeField]
	private TextMeshProUGUI _betteryLevelText;

	[SerializeField]
	private Image _gaugeImage;

	[SerializeField]
	private Image _chargingImage;

	[SerializeField]
	private Color _redBatteryColor;

	public void UpdateUI(int levelPer, bool isCharging)
	{
		_betteryLevelText.SetText(_format, levelPer);
		UpdateColor(levelPer);
		UpdateGauge(levelPer, isCharging);
	}

	private void UpdateGauge(int levelPer, bool isCharging)
	{
		_gaugeImage.fillAmount = (float)levelPer / 100f;
		SetActiveGauge(!isCharging);
		SetActiveCharging(isCharging);
	}

	private void SetActiveGauge(bool active)
	{
		if (_gaugeImage.gameObject.activeSelf != active)
		{
			_gaugeImage.gameObject.SetActive(active);
		}
	}

	private void SetActiveCharging(bool active)
	{
		if (_chargingImage.gameObject.activeSelf != active)
		{
			_chargingImage.gameObject.SetActive(active);
		}
	}

	private void UpdateColor(int levelPer)
	{
		if (levelPer <= _redBatteryLevelPer)
		{
			_betteryLevelText.color = _redBatteryColor;
			_gaugeImage.color = _redBatteryColor;
		}
		else
		{
			_betteryLevelText.color = Color.white;
			_gaugeImage.color = Color.white;
		}
	}
}
