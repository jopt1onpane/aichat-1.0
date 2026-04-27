using Bulbul;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Collider))]
public class KouPenguinAchievement : MonoBehaviour
{
	[Inject]
	private AchievementService _achievementService;

	private void OnMouseDown()
	{
		if (InputController.Instance.CurrentFrameEventSystemRaycastResult.Count <= 0)
		{
			_achievementService.OnClickKouPenguin();
		}
	}
}
