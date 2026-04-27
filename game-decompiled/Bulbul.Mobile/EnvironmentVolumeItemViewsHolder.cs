using Com.ForbiddenByte.OSA.Core;

namespace Bulbul.Mobile;

public class EnvironmentVolumeItemViewsHolder : BaseItemViewsHolder
{
	public EnvironmentVolumeItemView View { get; private set; }

	public override void CollectViews()
	{
		base.CollectViews();
		View = root.GetComponent<EnvironmentVolumeItemView>();
		View.Init();
	}
}
