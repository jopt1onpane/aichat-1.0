using UnityEngine;

namespace Bulbul;

public class PoorConnectionTooltipView : MonoBehaviour
{
	[SerializeField]
	private GameObject _cautionText;

	[SerializeField]
	private GameObject _tooltipTarget;

	public void Update()
	{
		bool flag = !_cautionText.activeSelf;
		if (_tooltipTarget.activeSelf != flag)
		{
			_tooltipTarget.SetActive(flag);
		}
	}
}
