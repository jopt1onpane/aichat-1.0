using UnityEngine;

namespace Bulbul.Mobile;

public class TodoTaskListCompleteItemView : MonoBehaviour
{
	[SerializeField]
	private TodoTaskListItemView view;

	[SerializeField]
	private RectTransform _rectTransform;

	public TodoTaskListItemView View => view;

	public RectTransform RectTransform => _rectTransform;

	public void UpdateView(TodoTaskListCompleteItemModel model, bool isRemovingMode)
	{
		if ((object)_rectTransform == null)
		{
			_rectTransform = base.transform as RectTransform;
		}
		view.CancelCompleteAndUnCompleteAnim();
		view.CancelRemovingAnimation();
		view.CreateRemovingAnimationCancellatonToken();
		view.Setup(model.UniqueID, model.Title, TodoState.Complete, model.Expire, model.CompletedDay, isRemovingMode);
	}

	public void ChangeRemovingMode(bool active)
	{
		view.ChangeRemovingMode(active, isImmediate: false);
	}
}
