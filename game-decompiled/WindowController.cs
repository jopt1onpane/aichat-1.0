using System;
using UnityEngine;

public class WindowController : MonoBehaviour
{
	public enum WindowState
	{
		Close,
		Open
	}

	public enum WindowAnimationType
	{
		PlayOpenWindow,
		PlayCloseWindow
	}

	private const float FirstOpenDelayMinutesMin = 30f;

	private const float FirstOpenDelayMinutesMax = 60f;

	private const float FirstCloseDelayMinutesMin = 60f;

	private const float FirstCloseDelayMinutesMax = 90f;

	private const float DefaultDelayMinutesMin = 180f;

	private const float DefaultDelayMinutesMax = 240f;

	private const string CloseAnimName = "CloseWindow";

	private const string OpenAnimName = "OpenWindow";

	private WindowState _currentWindowState;

	[Header("窓の操作")]
	[SerializeField]
	private Animator _windowAnimator;

	private DateTime _lastUseDateTime;

	private float _delayMinutes;

	private bool _isFirst;

	public WindowState CurrentWindowState => _currentWindowState;

	public void Setup()
	{
		_delayMinutes = UnityEngine.Random.Range(30f, 60f);
		_delayMinutes = 0f;
		_isFirst = true;
	}

	public bool IsCanOpenOrCloseWindow()
	{
		if ((DateTime.Now - _lastUseDateTime).TotalMinutes >= (double)_delayMinutes)
		{
			return true;
		}
		return false;
	}

	public HeroineAI.ActionStateType GetNextOpenOrCloseState()
	{
		if (_currentWindowState != WindowState.Open)
		{
			return HeroineAI.ActionStateType.WildOpenWindow;
		}
		return HeroineAI.ActionStateType.WildCloseWindow;
	}

	private void ChangeWindowAnimation(WindowAnimationType type)
	{
		_windowAnimator.ResetTrigger(WindowAnimationType.PlayOpenWindow.ToString());
		_windowAnimator.ResetTrigger(WindowAnimationType.PlayCloseWindow.ToString());
		_windowAnimator.SetTrigger(type.ToString());
	}

	public void PlayWindowAnimationOpenWindow()
	{
		ChangeWindowAnimation(WindowAnimationType.PlayOpenWindow);
		_currentWindowState = WindowState.Open;
		ChangeDelay();
	}

	public void PlayWindowAnimationCloseWindow()
	{
		ChangeWindowAnimation(WindowAnimationType.PlayCloseWindow);
		_currentWindowState = WindowState.Close;
		ChangeDelay();
	}

	public void ImmediateTidyingAnimation()
	{
		switch (_currentWindowState)
		{
		case WindowState.Open:
			_windowAnimator.Play("OpenWindow", 0, 1f);
			break;
		case WindowState.Close:
			_windowAnimator.Play("CloseWindow", 0, 1f);
			break;
		}
	}

	public void ChangeDelay()
	{
		if (_isFirst)
		{
			_delayMinutes = UnityEngine.Random.Range(60f, 90f);
			_isFirst = false;
		}
		else
		{
			_delayMinutes = UnityEngine.Random.Range(180f, 240f);
		}
		_lastUseDateTime = DateTime.Now;
	}
}
