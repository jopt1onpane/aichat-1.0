using UnityEngine;
using UnityEngine.UI;

namespace Games.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerAdjustor : MonoBehaviour
{
	[SerializeField]
	private float smoothAspectRange;

	private CanvasScaler _canvasScaler;

	private CanvasScaler canvasScaler
	{
		get
		{
			if (!_canvasScaler)
			{
				return _canvasScaler = GetComponent<CanvasScaler>();
			}
			return _canvasScaler;
		}
	}

	private void Update()
	{
		float num = (float)Screen.width / (float)Screen.height;
		float num2 = 1f - smoothAspectRange;
		float num3 = 1f + smoothAspectRange;
		if (num < num2)
		{
			canvasScaler.matchWidthOrHeight = 0f;
			return;
		}
		if (num > num3)
		{
			canvasScaler.matchWidthOrHeight = 1f;
			return;
		}
		float t = Mathf.InverseLerp(num2, num3, num);
		canvasScaler.matchWidthOrHeight = Mathf.Lerp(0f, 1f, t);
	}
}
