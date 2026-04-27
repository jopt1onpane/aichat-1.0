using UnityEngine;
using VContainer;

public class HeroineMotionAchievement : MonoBehaviour
{
	[Inject]
	private AchievementService _achievementService;

	public void OnEndDrinkHotReaction()
	{
		_achievementService.OnEndDrinkHotReaction();
	}
}
