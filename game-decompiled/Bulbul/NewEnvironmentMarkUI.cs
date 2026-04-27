using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class NewEnvironmentMarkUI : MonoBehaviour
{
	[SerializeField]
	private Image mark;

	public void Setup()
	{
		EnvironmentControllerBase[] environment = Object.FindObjectsByType<EnvironmentControllerBase>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToArray();
		Observable<bool>[] sources = environment.Select((EnvironmentControllerBase e) => e.IsNewIconActive).ToArray();
		ObservableSubscribeExtensions.Subscribe(Observable.Merge(sources), delegate
		{
			bool active2 = environment.Any((EnvironmentControllerBase e) => e.IsNewIconActive.CurrentValue);
			mark.gameObject.SetActive(active2);
		}).AddTo(this);
		bool active = environment.Any((EnvironmentControllerBase e) => e.IsNewIconActive.CurrentValue);
		mark.gameObject.SetActive(active);
	}
}
