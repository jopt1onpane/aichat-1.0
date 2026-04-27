using NestopiSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class MusicPlayingIconViewer : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve curve;

	[SerializeField]
	private Image[] icons;

	[SerializeField]
	private float speed = 1f;

	[SerializeField]
	private float minScale = 0.5f;

	[SerializeField]
	private float maxScale = 1f;

	private void Update()
	{
		foreach (var item3 in icons.Indexed())
		{
			Image item = item3.item;
			int item2 = item3.index;
			float num = 1f / (float)icons.Length * (float)item2;
			float t = curve.Evaluate(Mathf.Repeat(Time.time * speed + num, 1f));
			item.fillAmount = Mathf.Lerp(minScale, maxScale, t);
		}
	}
}
