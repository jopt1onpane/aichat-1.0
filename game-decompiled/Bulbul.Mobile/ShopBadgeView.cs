using UnityEngine;

namespace Bulbul.Mobile;

public class ShopBadgeView : MonoBehaviour
{
	[SerializeField]
	private GameObject badgeObj;

	public void SetActiveBadge(bool active)
	{
		if (badgeObj.activeSelf != active)
		{
			badgeObj.SetActive(active);
		}
	}
}
