using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class ToggleStyleButton : MonoBehaviour
{
	public enum SEStyle
	{
		None,
		Toggle,
		Toggle2,
		PullDown
	}

	private class DefaultTransition : IToggleStyleButtonTransition
	{
		private GameObject _on;

		private GameObject _off;

		bool IToggleStyleButtonTransition.IsTrantioning => false;

		public DefaultTransition(GameObject on, GameObject off)
		{
			_on = on;
			_off = off;
		}

		void IToggleStyleButtonTransition.Transition(bool isOn, bool isImmediate = false)
		{
			if (isOn)
			{
				_on.SetActive(value: true);
				_off.SetActive(value: false);
			}
			else
			{
				_on.SetActive(value: false);
				_off.SetActive(value: true);
			}
		}
	}

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	[Header("OFF状態の時のボタン")]
	private Button _offStateButton;

	[SerializeField]
	[Header("ON状態の時のボタン")]
	private Button _onStateButton;

	[SerializeField]
	[Header("ボタンのSE Noneで無音")]
	private SEStyle _seStyle;

	[SerializeField]
	[Header("Enableを切っている時の表示(OFF)")]
	private Image _disableCoverOff;

	[SerializeField]
	[Header("Enableを切っている時の表示(ON)")]
	private Image _disableCoverOn;

	private Subject<bool> _onValueChanged = new Subject<bool>();

	private IToggleStyleButtonTransition __transition;

	public Observable<bool> OnValueChanged => _onValueChanged;

	public bool IsOn { get; private set; }

	private IToggleStyleButtonTransition _transition
	{
		get
		{
			if (__transition == null && !TryGetComponent<IToggleStyleButtonTransition>(out __transition))
			{
				__transition = new DefaultTransition(_onStateButton.gameObject, _offStateButton.gameObject);
			}
			return __transition;
		}
	}

	public void SetEnable(bool enable)
	{
		_offStateButton.enabled = enable;
		_onStateButton.enabled = enable;
		if (_disableCoverOff != null)
		{
			_disableCoverOff.enabled = !enable;
		}
		if (_disableCoverOn != null)
		{
			_disableCoverOn.enabled = !enable;
		}
	}

	public void Awake()
	{
		if (_systemSeService == null)
		{
			_systemSeService = RoomLifetimeScope.Resolve<SystemSeService>();
		}
		ObservableSubscribeExtensions.Subscribe(_offStateButton.OnClickAsObservable(), delegate
		{
			TryPlaySE(isOn: true);
			SetToggle(isOn: true);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_onStateButton.OnClickAsObservable(), delegate
		{
			TryPlaySE(isOn: false);
			SetToggle(isOn: false);
		}).AddTo(this);
		SetEnable(enable: true);
	}

	private bool TryPlaySE(bool isOn)
	{
		switch (_seStyle)
		{
		case SEStyle.None:
			return false;
		case SEStyle.PullDown:
			if (isOn)
			{
				_systemSeService?.PlayPulldownOpen();
			}
			else
			{
				_systemSeService?.PlayPulldownClose();
			}
			return true;
		case SEStyle.Toggle:
			if (isOn)
			{
				_systemSeService?.PlayClick();
			}
			else
			{
				_systemSeService?.PlayClick();
			}
			return true;
		case SEStyle.Toggle2:
			if (isOn)
			{
				_systemSeService?.PlaySelect();
			}
			else
			{
				_systemSeService?.PlaySelect();
			}
			return true;
		default:
			return false;
		}
	}

	public void SetToggleWithoutTransition(bool isOn, bool isNotify)
	{
		_transition.Transition(isOn, isImmediate: true);
		IsOn = isOn;
		if (isNotify)
		{
			_onValueChanged.OnNext(isOn);
		}
	}

	private void SetToggle(bool isOn)
	{
		if (!_transition.IsTrantioning)
		{
			IsOn = isOn;
			_transition.Transition(isOn);
			_onValueChanged.OnNext(isOn);
		}
	}
}
