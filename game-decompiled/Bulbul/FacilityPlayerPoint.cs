using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class FacilityPlayerPoint : MonoBehaviour
{
	[Inject]
	private PlayerPointService _playerPointService;

	[Inject]
	private IPlayerPointGetUI _playerPointUI;

	public void Setup()
	{
		_playerPointUI.Setup(_playerPointService.Point);
		_playerPointService.OnPointChange.Subscribe(delegate(int pointDiff)
		{
			_playerPointUI.ChangePoint(pointDiff, _playerPointService.Point);
		}).AddTo(this);
	}
}
