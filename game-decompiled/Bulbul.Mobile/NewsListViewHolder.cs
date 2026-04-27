using Com.ForbiddenByte.OSA.Core;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class NewsListViewHolder : BaseItemViewsHolder
{
	private NewsListItemView _view;

	private ContentSizeFitter _csf;

	public NewsListItemView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<NewsListItemView>();
			}
			return _view;
		}
	}

	public ContentSizeFitter CSF
	{
		get
		{
			if (_csf == null)
			{
				_csf = root.GetComponent<ContentSizeFitter>();
			}
			return _csf;
		}
	}

	public override void CollectViews()
	{
		base.CollectViews();
		View.Setup();
	}

	public override void MarkForRebuild()
	{
		base.MarkForRebuild();
		if ((bool)CSF)
		{
			CSF.enabled = true;
		}
	}

	public override void UnmarkForRebuild()
	{
		if ((bool)CSF)
		{
			CSF.enabled = false;
		}
		base.UnmarkForRebuild();
	}

	public void SetModel(NewsListItemModel model)
	{
		View.SetModel(model);
	}
}
