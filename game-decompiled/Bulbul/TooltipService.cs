using UnityEngine;

namespace Bulbul;

public class TooltipService : MonoBehaviour
{
	private enum DelayState
	{
		None,
		WaitActivate,
		WaitDeactivate
	}

	[SerializeField]
	private TooltipUI _tooltipUI;

	[SerializeField]
	private float _activateDelay;

	[SerializeField]
	private float _deactivateDelay;

	private TooltipTarget _currentTarget;

	private string _currentLocalizeID;

	private float _targetElapsedTime;

	private DelayState _delayState;

	public void Setup()
	{
		_tooltipUI.Setup();
	}

	public void UpdateTarget(TooltipTarget target)
	{
		TooltipTarget currentTarget = _currentTarget;
		string currentLocalizeID = _currentLocalizeID;
		_currentTarget = target;
		_currentLocalizeID = target?.Data.ContentLocalizeID;
		if (_currentTarget != currentTarget)
		{
			UpdateTips();
		}
		else if (_delayState == DelayState.None && currentTarget != null && _currentTarget == currentTarget && currentLocalizeID != null && _currentLocalizeID != null && currentLocalizeID != _currentLocalizeID)
		{
			UpdateTips();
		}
		if (_delayState != DelayState.None)
		{
			_targetElapsedTime += Time.unscaledDeltaTime;
			if (_delayState == DelayState.WaitActivate && _targetElapsedTime >= _activateDelay)
			{
				_delayState = DelayState.None;
				_tooltipUI.Activate();
			}
			else if (_delayState == DelayState.WaitDeactivate && _targetElapsedTime >= _deactivateDelay)
			{
				_delayState = DelayState.None;
				_tooltipUI.Deactivate();
			}
		}
		if (_tooltipUI.IsActive && _delayState != DelayState.WaitDeactivate)
		{
			_tooltipUI.SetPosition(InputController.Instance.GetInputPos());
		}
		void UpdateTips()
		{
			bool flag = _currentTarget != null;
			if (flag)
			{
				flag = _tooltipUI.SetData(_currentTarget.Data);
			}
			if (flag)
			{
				_delayState = ((!_tooltipUI.IsActive) ? DelayState.WaitActivate : DelayState.None);
			}
			else
			{
				_delayState = ((!_tooltipUI.IsInactive) ? DelayState.WaitDeactivate : DelayState.None);
			}
			_targetElapsedTime = 0f;
		}
	}

	public void DeactivateOnCurrentTarget()
	{
		if (_delayState != DelayState.WaitDeactivate && !_tooltipUI.IsInactive)
		{
			_tooltipUI.Deactivate();
			_delayState = DelayState.None;
		}
	}
}
