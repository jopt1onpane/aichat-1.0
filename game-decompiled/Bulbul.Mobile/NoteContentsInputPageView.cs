using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class NoteContentsInputPageView : MonoBehaviour
{
	[SerializeField]
	private NoteContentInputFieldsView _inputFieldsView;

	[SerializeField]
	private Button _returnButton;

	public NoteContentInputFieldsView InputFieldsView => _inputFieldsView;

	public Observable<Unit> OnClickReturnButton => _returnButton.OnClickAsObservable();

	public void Setup()
	{
		_inputFieldsView.Setup();
	}

	public void ResetScroll()
	{
		_inputFieldsView.ResetScroll();
	}

	public void StartTitleInput()
	{
		_inputFieldsView.StartTitleInput();
	}

	public void StartMainInput()
	{
		_inputFieldsView.StartMainInput();
	}
}
