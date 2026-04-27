using Com.ForbiddenByte.OSA.Core;

namespace Bulbul.Mobile;

public class DecorationSkinListItemViewForMobileHolder : BaseItemViewsHolder
{
	public DecorationSkinListItemViewForMobile View { get; private set; }

	public override void CollectViews()
	{
		base.CollectViews();
		View = root.GetComponent<DecorationSkinListItemViewForMobile>();
	}

	public void UpdateView(DecorationSkinListItemModelForMobile model)
	{
		View.SetSkin(model.skinType, model.DeactivationType);
		View.SetPriceData(model.Price, model.IsPurchased);
		View.SetActiveNewObj(model.IsNew);
		View.SetSelected(model.IsSelected);
		View.SetActiveMobileDemoEditionLockedObj(model.IsMobileDemoEditionLocked);
	}
}
