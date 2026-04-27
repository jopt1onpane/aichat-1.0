using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

[RequireComponent(typeof(Graphic))]
[DisallowMultipleComponent]
public class ColorConstraint : MonoBehaviour
{
	[Header("Source Selection")]
	[SerializeField]
	private Graphic sourceGraphic;

	[SerializeField]
	[Range(0f, 1f)]
	private float weight = 1f;

	[Header("Channels")]
	public bool syncRGB = true;

	public bool syncAlpha = true;

	private Graphic targetGraphic;

	private void OnEnable()
	{
		targetGraphic = GetComponent<Graphic>();
	}

	private void LateUpdate()
	{
		if (!(sourceGraphic == null) && !(targetGraphic == null))
		{
			Color color = sourceGraphic.color;
			Color color2 = targetGraphic.color;
			Color color3 = color2;
			if (syncRGB)
			{
				color3.r = Mathf.Lerp(color2.r, color.r, weight);
				color3.g = Mathf.Lerp(color2.g, color.g, weight);
				color3.b = Mathf.Lerp(color2.b, color.b, weight);
			}
			if (syncAlpha)
			{
				color3.a = Mathf.Lerp(color2.a, color.a, weight);
			}
			targetGraphic.color = color3;
		}
	}
}
