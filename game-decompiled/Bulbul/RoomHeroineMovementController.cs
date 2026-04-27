using System;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Bulbul;

public class RoomHeroineMovementController
{
	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private RoomCameraManager _roomCameraManager;

	public void OnInput(RoomHeroineMovementParam parameter, Action onChangeEnd = null)
	{
		_heroineService.ChangeViewPoint(parameter.MovementNo, onChangeEnd).Forget();
	}
}
