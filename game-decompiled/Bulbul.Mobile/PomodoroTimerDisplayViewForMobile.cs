using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class PomodoroTimerDisplayViewForMobile : MonoBehaviour
{
	private static readonly string inputStr = "{0}";

	private static readonly string remainCountInputStr = "{0}/{1}";

	[SerializeField]
	private TextMeshProUGUI _settingLoopCountText;

	[SerializeField]
	private TextMeshProUGUI _settingTimeText;

	[SerializeField]
	private TextMeshProUGUI _remainLoopCountText;

	[SerializeField]
	private TextMeshProUGUI _timeText;

	[SerializeField]
	private Image _meterOverWriteImage;

	[SerializeField]
	private Button _openSettingButton;

	private IPomodoroTimerStateView _stateView;

	public Observable<Unit> OnClickOpenSettingButton
	{
		get
		{
			if (!(_openSettingButton != null))
			{
				return null;
			}
			return _openSettingButton.OnClickAsObservable();
		}
	}

	public IPomodoroTimerStateView StateView
	{
		get
		{
			if (_stateView == null)
			{
				_stateView = GetComponentInChildren<IPomodoroTimerStateView>(includeInactive: true);
				_stateView.Setup();
			}
			return _stateView;
		}
	}

	public void SetSettingLoopCountText(int count)
	{
		_settingLoopCountText.SetText(remainCountInputStr, 0f, count);
	}

	public void SetRemainLoopCountText(int remainCount, int baseCount)
	{
		_remainLoopCountText.SetText(remainCountInputStr, remainCount, baseCount);
	}

	public void SetTimeText(string text)
	{
		_timeText.SetText(text);
	}

	public void SetSettingTimeText(string text)
	{
		_settingTimeText.SetText(text);
	}

	public void SetMeterOverWriteImageFillAmount(float amount)
	{
		_meterOverWriteImage.fillAmount = amount;
	}

	public void DeactivateOpenButton()
	{
		if (!(_openSettingButton == null))
		{
			_openSettingButton.enabled = false;
		}
	}

	public void ActivateOpenButton()
	{
		if (!(_openSettingButton == null))
		{
			_openSettingButton.enabled = true;
		}
	}

	public void SetInteractableOpenButton(bool active)
	{
		if (!(_openSettingButton == null))
		{
			_openSettingButton.interactable = active;
		}
	}
}
