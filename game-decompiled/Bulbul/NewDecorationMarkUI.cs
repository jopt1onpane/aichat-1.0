using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class NewDecorationMarkUI : MonoBehaviour
{
	[SerializeField]
	private Image mark;

	public void Setup()
	{
		DecorationButtonUI[] decorations = Object.FindObjectsByType<DecorationButtonUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToArray();
		ObservableSubscribeExtensions.Subscribe(decorations.Select((DecorationButtonUI d) => d.IsNewIconActive).Merge(), delegate
		{
			bool active2 = decorations.Any((DecorationButtonUI d) => d.IsNewIconActive.CurrentValue);
			mark.gameObject.SetActive(active2);
		}).AddTo(this);
		bool active = decorations.Any((DecorationButtonUI d) => d.IsNewIconActive.CurrentValue);
		mark.gameObject.SetActive(active);
	}
}
