using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class TodoTaskListCompletePullDownView : MonoBehaviour
{
	[SerializeField]
	private Button pullDownButton;

	[SerializeField]
	private ToggleStyleButton pullDownToggleButton;

	public ToggleStyleButton PullDownButton => pullDownToggleButton;

	private void Start()
	{
		ObservableSubscribeExtensions.Subscribe(pullDownButton.OnClickAsObservable(), delegate
		{
			pullDownToggleButton.SetToggleWithoutTransition(!pullDownToggleButton.IsOn, isNotify: true);
		}).AddTo(this);
	}

	public void UpdateView(TodoTaskListCompletePullDownModel model)
	{
		pullDownToggleButton.SetToggleWithoutTransition(model.IsOpened, isNotify: false);
		pullDownToggleButton.SetEnable(!model.IsRemoving && !model.IsTodoStateChanging);
	}
}
