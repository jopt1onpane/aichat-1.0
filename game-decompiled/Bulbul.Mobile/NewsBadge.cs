using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class NewsBadge : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _badgeCanvas;

	private void Start()
	{
		InMemoryData.GetOrSet(() => new NewsState()).AvailableNewNews.Subscribe(delegate(bool isOn)
		{
			_badgeCanvas.alpha = (isOn ? 1f : 0f);
		}).AddTo(this);
	}
}
