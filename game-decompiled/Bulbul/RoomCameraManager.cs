using System;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bulbul;

public class RoomCameraManager : MonoBehaviour
{
	public enum AnimationType
	{
		Desk_Camera_Setting,
		Count
	}

	public enum VirtualCameraType
	{
		Initial,
		Base,
		Chair,
		Desk,
		CoffeeMaker,
		Window,
		StoryChairFaceToFace,
		StoryIntroduceBook,
		Sofa,
		DecorationEdit,
		DecorationEditChair,
		DecorationEditDesk,
		WallPaperLandscape,
		StoryLandscape,
		MobileInitial,
		MAX
	}

	private readonly int StartCameraAnimHash = Animator.StringToHash("anim_startcamera_pc");

	private readonly int StartCameraMobileAnimHash = Animator.StringToHash("anim_startcamera_mobile");

	[SerializeField]
	private Camera _cam;

	[SerializeField]
	private Transform[] _camTransform;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private CinemachineBrain _brain;

	[SerializeField]
	private GameObject _decorationCam;

	[SerializeField]
	private GameObject _decorationChairCam;

	[SerializeField]
	private GameObject _decorationDeskCam;

	[SerializeField]
	private GameObject _wallPaperLandscapeCam;

	[SerializeField]
	private GameObject _storyLandscapeCam;

	[SerializeField]
	private GameObject[] _virtualCameras;

	private GameObject prevCam;

	private GameObject currentCam;

	private VirtualCameraType currentVirtualCameraType;

	private bool isPlayingStory;

	private int _usedStartCameraAnimHash;

	private int? _overwriteStartCameraAnimHash;

	public bool IsBlending => _brain.IsBlending;

	private void Update()
	{
		if (!DevicePlatform.Steam.IsPC())
		{
			UpdateStoryCamera();
		}
	}

	public void Setup()
	{
		if (DevicePlatform.Steam.IsPC())
		{
			_usedStartCameraAnimHash = StartCameraAnimHash;
			_cam.fieldOfView = 35f;
			_cam.farClipPlane = 1000f;
		}
		else
		{
			_usedStartCameraAnimHash = StartCameraMobileAnimHash;
			_cam.fieldOfView = 45f;
			_cam.farClipPlane = 1500f;
			VirtualCameraSwitch(VirtualCameraType.MobileInitial);
		}
	}

	public void ChangeCameraTransform(int transformIndex)
	{
		if (transformIndex >= _camTransform.Length)
		{
			Debug.LogError($"positionNo is over positionNo:{transformIndex} _camPositions.Length:{_camTransform.Length}");
			return;
		}
		Transform transform = _camTransform[transformIndex];
		_cam.transform.SetPositionAndRotation(transform.position, transform.rotation);
	}

	public void PlayCameraAnimation(AnimationType animationType)
	{
		SetEnableCameraBlend(isEnable: false);
		VirtualCameraSwitch(VirtualCameraType.MobileInitial);
		_animator.enabled = true;
		if (_overwriteStartCameraAnimHash.HasValue)
		{
			_animator.Play(_overwriteStartCameraAnimHash.Value, 0, 0f);
		}
		else
		{
			_animator.Play(_usedStartCameraAnimHash, 0, 0f);
		}
	}

	public void EndTidying()
	{
		_animator.Rebind();
		_animator.enabled = false;
	}

	public void SkipCameraBlend()
	{
		if (DevicePlatform.Steam.IsMobile() && _brain.IsBlending)
		{
			_brain.ActiveBlend = null;
		}
	}

	public void SetEnableCameraBlend(bool isEnable)
	{
		DevicePlatform.Steam.IsMobile();
	}

	public void VirtualCameraSwitch(VirtualCameraType cameraType)
	{
		if (!DevicePlatform.Steam.IsPC() && currentVirtualCameraType != cameraType)
		{
			if (currentCam != null)
			{
				prevCam = currentCam;
			}
			currentCam = _virtualCameras[(int)cameraType];
			currentCam.SetActive(value: true);
			currentVirtualCameraType = cameraType;
			if (prevCam != null)
			{
				prevCam.SetActive(value: false);
			}
		}
	}

	public void SetActiveDecorationCam(bool isActive, Action onComplete = null)
	{
		if (!DevicePlatform.Steam.IsPC())
		{
			_decorationCam.SetActive(isActive);
		}
	}

	public void SetActiveDecorationChairCam(bool isActive)
	{
		if (_decorationChairCam.activeSelf != isActive)
		{
			_decorationChairCam.SetActive(isActive);
		}
	}

	public void SetActiveDecorationDeskCam(bool isActive)
	{
		if (_decorationDeskCam.activeSelf != isActive)
		{
			_decorationDeskCam.SetActive(isActive);
		}
	}

	public void SetActiveWallPaperLandScapeCam(bool isActive)
	{
		if (_wallPaperLandscapeCam.activeSelf != isActive)
		{
			_wallPaperLandscapeCam.SetActive(isActive);
		}
	}

	public void SetActiveLandScapeStoryCam(bool isActive)
	{
		if (_storyLandscapeCam.activeSelf != isActive)
		{
			_storyLandscapeCam.SetActive(isActive);
		}
	}

	private async UniTaskVoid DecorationCamBlendCompleteCheck(Action onComplete)
	{
		await UniTask.Yield();
		CinemachineBlend blend = _brain.ActiveBlend;
		if (blend == null)
		{
			onComplete?.Invoke();
			return;
		}
		ICinemachineCamera camA = blend.CamA;
		_ = blend.CamB;
		if (camA != null)
		{
		}
		await UniTask.WaitUntil(() => blend != null && blend.IsComplete);
		onComplete?.Invoke();
	}

	public void OnStartStory()
	{
		if (!DevicePlatform.Steam.IsPC())
		{
			isPlayingStory = true;
			GameObject[] virtualCameras = _virtualCameras;
			for (int i = 0; i < virtualCameras.Length; i++)
			{
				virtualCameras[i].gameObject.SetActive(value: false);
			}
			GameObject gameObject = _virtualCameras[1];
			currentVirtualCameraType = VirtualCameraType.Base;
			currentCam = gameObject;
			gameObject.SetActive(value: true);
			_cam.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);
			_cam.fieldOfView = 35f;
		}
	}

	public void OnEndStory()
	{
		if (DevicePlatform.Steam.IsPC())
		{
			return;
		}
		_overwriteStartCameraAnimHash = null;
		isPlayingStory = false;
		GameObject gameObject = _virtualCameras[1];
		_cam.transform.SetPositionAndRotation(_cam.transform.position, gameObject.transform.rotation);
		_cam.fieldOfView = 45f;
		currentCam = _virtualCameras[1];
		currentCam.SetActive(value: true);
		currentVirtualCameraType = VirtualCameraType.Base;
		GameObject[] virtualCameras = _virtualCameras;
		foreach (GameObject gameObject2 in virtualCameras)
		{
			if (gameObject2 != currentCam)
			{
				gameObject2.SetActive(value: false);
			}
		}
	}

	private void UpdateStoryCamera()
	{
		if (isPlayingStory)
		{
			ScreenOrientation orientation = Screen.orientation;
			if ((uint)(orientation - 3) <= 1u)
			{
				SetActiveLandScapeStoryCam(isActive: true);
				_overwriteStartCameraAnimHash = StartCameraAnimHash;
			}
			else
			{
				SetActiveLandScapeStoryCam(isActive: false);
				_overwriteStartCameraAnimHash = StartCameraMobileAnimHash;
			}
		}
	}
}
