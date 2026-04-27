namespace Bulbul.Mobile;

public class DecorationSkinListItemModelForMobile
{
	public enum DeactivationCategoryType
	{
		None,
		Glass,
		Other
	}

	public DecorationService.DecorationSkinType skinType;

	public bool IsNew;

	public int Price;

	public bool IsPurchased;

	public bool IsSelected;

	public DeactivationCategoryType DeactivationType;

	public bool IsMobileDemoEditionLocked;
}
