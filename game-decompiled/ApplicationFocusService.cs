using UnityEngine;

public class ApplicationFocusService
{
	private const float FocusChangeIgnoreDuration = 0.1f;

	private bool _isFocusedOneSecondAgo;

	private float _lastFocusChangeTime = -999f;

	public bool IsFocusedOneSecondAgo => _isFocusedOneSecondAgo;

	public void UpdateBeforeIsFocus(bool isFocus)
	{
		_isFocusedOneSecondAgo = isFocus;
	}

	public void OnFocusChanged(bool hasFocus)
	{
		if (hasFocus)
		{
			_lastFocusChangeTime = Time.realtimeSinceStartup;
		}
	}

	public bool IsJustAfterFocusChanged()
	{
		return Time.realtimeSinceStartup - _lastFocusChangeTime < 0.1f;
	}
}
