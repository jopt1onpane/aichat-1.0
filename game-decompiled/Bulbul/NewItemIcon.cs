using UnityEngine;

namespace Bulbul;

public class NewItemIcon : MonoBehaviour
{
	public void ActivateIcon()
	{
		base.gameObject.SetActive(value: true);
	}

	public void DeactivateIcon()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetIconActive(bool active)
	{
		if (active)
		{
			ActivateIcon();
		}
		else
		{
			DeactivateIcon();
		}
	}
}
