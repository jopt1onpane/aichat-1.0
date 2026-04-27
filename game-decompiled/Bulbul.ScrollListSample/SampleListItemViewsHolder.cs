using Com.ForbiddenByte.OSA.Core;

namespace Bulbul.ScrollListSample;

public class SampleListItemViewsHolder : BaseItemViewsHolder
{
	private SampleListItemView _view;

	public override void CollectViews()
	{
		base.CollectViews();
		_view = root.GetComponent<SampleListItemView>();
	}

	public void SetModel(SampleItemModel model)
	{
		_view.TitleText.text = model.Title;
	}

	public void UnsetModel()
	{
	}
}
