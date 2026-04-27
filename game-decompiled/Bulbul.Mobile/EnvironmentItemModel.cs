namespace Bulbul.Mobile;

public class EnvironmentItemModel
{
	public enum ItemLockState
	{
		Unlocked,
		Locked,
		LockedByPurchase
	}

	public bool IsMobileDemoEditionLocked;

	public EnvironmentType EnvironmentType { get; init; }

	public int SortOrder { get; init; }

	public string NameLocalizeID { get; init; }

	public bool IsWindowActive { get; set; }

	public bool IsSoundActive => Volume > 0f;

	public float Volume { get; set; }

	public bool IsNew { get; set; }

	public ItemLockState LockState { get; set; }

	public int Price { get; init; }

	public bool HasEnoughPoints { get; set; }
}
