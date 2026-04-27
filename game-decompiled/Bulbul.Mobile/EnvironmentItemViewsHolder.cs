using Com.ForbiddenByte.OSA.CustomAdapters.GridView;

namespace Bulbul.Mobile;

public class EnvironmentItemViewsHolder : CellViewsHolder
{
	public EnvironmentItemView View { get; private set; }

	public override void CollectViews()
	{
		base.CollectViews();
		View = root.GetComponent<EnvironmentItemView>();
		View.Init();
	}
}
