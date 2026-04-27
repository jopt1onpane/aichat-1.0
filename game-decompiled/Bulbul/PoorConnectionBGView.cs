using TMPro;
using UnityEngine;

namespace Bulbul;

public class PoorConnectionBGView : MonoBehaviour
{
	[SerializeField]
	private TMP_Text cautionText;

	[SerializeField]
	private float baseWidth;

	[SerializeField]
	private RectTransform bgRecttransform;

	private void Update()
	{
		float preferredWidth = cautionText.preferredWidth;
		bgRecttransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, baseWidth + preferredWidth);
	}
}
