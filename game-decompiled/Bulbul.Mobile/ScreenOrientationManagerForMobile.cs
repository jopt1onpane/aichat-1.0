using UnityEngine;

namespace Bulbul.Mobile;

public class ScreenOrientationManagerForMobile
{
	public enum LockOrientationType
	{
		None,
		LandScape,
		Portrait,
		All
	}

	private static ScreenOrientationManagerForMobile _instance;

	private LockOrientationType _currentLockType;

	public static ScreenOrientationManagerForMobile Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new ScreenOrientationManagerForMobile();
			}
			return _instance;
		}
	}

	public bool IsRotateLock { get; private set; }

	public bool IsTemporaryLock { get; private set; }

	public LockOrientationType CurrentLockType => _currentLockType;

	public void LockRotatePortrait()
	{
		ScreenOrientation orientation = Screen.orientation;
		if ((uint)(orientation - 3) <= 1u)
		{
			Screen.orientation = ScreenOrientation.Portrait;
		}
		Screen.autorotateToPortrait = true;
		Screen.autorotateToPortraitUpsideDown = true;
		Screen.autorotateToLandscapeLeft = false;
		Screen.autorotateToLandscapeRight = false;
		Screen.orientation = ScreenOrientation.AutoRotation;
		IsRotateLock = true;
		_currentLockType = LockOrientationType.Portrait;
	}

	public void LockRotateLandscape()
	{
		ScreenOrientation orientation = Screen.orientation;
		if ((uint)(orientation - 1) <= 1u)
		{
			Screen.orientation = ScreenOrientation.LandscapeLeft;
		}
		Screen.autorotateToLandscapeLeft = true;
		Screen.autorotateToLandscapeRight = true;
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		Screen.orientation = ScreenOrientation.AutoRotation;
		IsRotateLock = true;
		_currentLockType = LockOrientationType.LandScape;
	}

	public void LockCurrentRotateType()
	{
		ScreenOrientation orientation = Screen.orientation;
		if ((uint)(orientation - 1) <= 1u)
		{
			LockRotatePortrait();
		}
		else
		{
			LockRotateLandscape();
		}
	}

	public void SetAutoRotateFree()
	{
		Screen.autorotateToLandscapeLeft = true;
		Screen.autorotateToLandscapeRight = true;
		Screen.autorotateToPortrait = true;
		Screen.autorotateToPortraitUpsideDown = true;
		IsRotateLock = false;
		_currentLockType = LockOrientationType.None;
	}

	public void SetAutoRotateLock()
	{
		Screen.autorotateToLandscapeLeft = false;
		Screen.autorotateToLandscapeRight = false;
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		IsRotateLock = true;
		_currentLockType = LockOrientationType.All;
	}

	public void SetAutoRotateTemporaryLock(bool isLock)
	{
		if (isLock)
		{
			Screen.autorotateToLandscapeLeft = false;
			Screen.autorotateToLandscapeRight = false;
			Screen.autorotateToPortrait = false;
			Screen.autorotateToPortraitUpsideDown = false;
			IsTemporaryLock = true;
			return;
		}
		IsTemporaryLock = false;
		switch (_currentLockType)
		{
		case LockOrientationType.None:
			SetAutoRotateFree();
			break;
		case LockOrientationType.Portrait:
			LockRotatePortrait();
			break;
		case LockOrientationType.LandScape:
			LockRotateLandscape();
			break;
		}
	}
}
