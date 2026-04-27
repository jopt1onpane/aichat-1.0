using UnityEngine;

namespace Bulbul.Mobile;

public class EnvironmentIconDBMobile : EnvironmentIconDBBase<EnvironmentIconEntryMobile>
{
	public (Sprite icon, Sprite iconActive) GetMainButtonIcon(EnvironmentType environmentType)
	{
		EnvironmentIconEntryMobile entry = GetEntry(environmentType);
		if (entry == null)
		{
			return (icon: null, iconActive: null);
		}
		return (icon: entry.MainButtonIcon, iconActive: entry.MainButtonIconActive);
	}

	public Sprite GetShopIcon(EnvironmentType environmentType)
	{
		return GetEntry(environmentType)?.ShopIcon;
	}
}
