using Bulbul;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

public class PhotographyToolService : MonoBehaviour
{
	[Inject]
	private HeroineService _heroineService;

	[SerializeField]
	private PlayableDirector workDirector;

	[SerializeField]
	private PlayableDirector breakDirector;

	private PlayableDirector currentDirector;

	public void PlayTimeLine(float startTime, HeroineAI.ActionType actionType)
	{
		currentDirector = null;
		if (currentDirector != null)
		{
			currentDirector.Stop();
		}
		switch (actionType)
		{
		case HeroineAI.ActionType.Work:
			currentDirector = workDirector;
			break;
		case HeroineAI.ActionType.Break:
			currentDirector = breakDirector;
			break;
		}
		if (!(currentDirector == null))
		{
			_heroineService.DebugChangeState(HeroineAI.ActionStateType.MainStory);
			currentDirector.Pause();
			currentDirector.time = startTime;
			currentDirector.Evaluate();
			currentDirector.Play();
		}
	}

	public void StopTimeLine()
	{
		workDirector.Stop();
		breakDirector.Stop();
	}
}
