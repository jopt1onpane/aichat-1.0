using UnityEngine;

namespace Bulbul.Mobile;

public class TodoTaskListTaskItemView : MonoBehaviour
{
	[SerializeField]
	private TodoTaskListItemView view;

	[SerializeField]
	private RectTransform _rectTransform;

	public TodoTaskListItemView View => view;

	public RectTransform RectTransform => _rectTransform;

	public void UpdateView(TodoTaskListItemModel model, bool isRemovingMode, bool isPlaceHolder = false)
	{
		if ((object)_rectTransform == null)
		{
			_rectTransform = base.transform as RectTransform;
		}
		view.SetActive(!isPlaceHolder);
		if (!isPlaceHolder)
		{
			view.CancelRemovingAnimation();
			view.CreateRemovingAnimationCancellatonToken();
			view.Setup(model.UniqueID, model.Title, TodoState.Working, model.Expire, null, isRemovingMode);
			if (model.IsCreateNew)
			{
				view.EditMainText();
				model.IsCreateNew = false;
			}
		}
	}

	public void ChangeRemovingMode(bool active)
	{
		view.ChangeRemovingMode(active, isImmediate: false);
	}
}
