using Com.ForbiddenByte.OSA.Core;
using R3;

namespace Bulbul.Mobile;

public class TodoListSelectorListItemViewHolder : BaseItemViewsHolder
{
	private readonly ReactiveProperty<bool> gate;

	private TodoListSelectorListItemView _view;

	public TodoListSelectorListItemView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<TodoListSelectorListItemView>();
			}
			return _view;
		}
	}

	public TodoListSelectorListItemViewHolder(ReactiveProperty<bool> gate)
	{
		this.gate = gate;
	}

	public override void CollectViews()
	{
		base.CollectViews();
		View.Initialize(gate);
	}

	public void UpdateModel(TodoListSelectorListItemModel model, bool isRemovable = false)
	{
		View.SetModel(model, isRemovable);
	}
}
