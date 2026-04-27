using Bulbul;
using UnityEngine;
using VContainer;

public class GameStartCameraDirection : MonoBehaviour
{
	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private RoomCameraManager _roomCameraManager;

	[SerializeField]
	private GameObject _gameStartDirectionVolumeObject;

	public void Setup()
	{
		EndTidying();
	}

	public void Play()
	{
		GameStartAnimationForCameraTouch();
	}

	private void GameStartAnimationForCameraTouch()
	{
		_heroineService.ChangeHeroineAnimationImmediately(400);
		_roomCameraManager.ChangeCameraTransform(0);
		_roomCameraManager.PlayCameraAnimation(RoomCameraManager.AnimationType.Desk_Camera_Setting);
		_gameStartDirectionVolumeObject.SetActive(value: true);
		_systemSeService.PlayGameStartCameraNoise();
	}

	public void Stop()
	{
		_systemSeService.Stop(SystemSeType.GameStartCameraNoise);
		_gameStartDirectionVolumeObject.SetActive(value: false);
	}

	public void EndTidying()
	{
		_roomCameraManager.EndTidying();
		Stop();
	}
}
