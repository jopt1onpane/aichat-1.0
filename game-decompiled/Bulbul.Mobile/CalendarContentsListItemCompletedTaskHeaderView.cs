using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class CalendarContentsListItemCompletedTaskHeaderView : MonoBehaviour
{
	[SerializeField]
	private ToggleStyleButton _removingToggle;

	[SerializeField]
	private ToggleStyleButton _pulldown;

	[SerializeField]
	private Button _pulldownButton;

	public ToggleStyleButton RemovingToggle => _removingToggle;

	public ToggleStyleButton PullDown => _pulldown;

	private void Start()
	{
		ObservableSubscribeExtensions.Subscribe(_pulldownButton.OnClickAsObservable(), delegate
		{
			_pulldown.SetToggleWithoutTransition(!_pulldown.IsOn, isNotify: true);
		}).AddTo(this);
	}

	public void UpdateView(CalendarCompletedTaskHeaderViewModel model)
	{
		_removingToggle.SetToggleWithoutTransition(model.IsRemovingMode, isNotify: false);
		_pulldown.SetToggleWithoutTransition(model.IsOpened, isNotify: false);
		_removingToggle.gameObject.SetActive(model.IsOpened && model.ExistTodo);
		_pulldown.SetEnable(!model.IsRemoving);
	}

	private void Update()
	{
		_pulldownButton.enabled = _pulldown.enabled && _pulldown.gameObject.activeSelf;
	}
}
