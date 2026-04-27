using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class NoteContentInputFieldsView : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField _titleInputField;

	[SerializeField]
	private AutoSizingHeightInputFieldView _mainInputField;

	[SerializeField]
	private ScrollRect _mainInputScroll;

	private CancellationTokenSource _offsetScrollFromDiaryChangedHeightCancelletionToken;

	public Observable<string> OnValueChangeTitle => _titleInputField.OnValueChangedAsObservable();

	public Observable<string> OnEndEditTitle => _titleInputField.OnEndEditAsObservable();

	public Observable<string> OnValueChangeMain => _mainInputField.OnValueChanged;

	public Observable<string> OnEndEditMain => _mainInputField.OnEndEdit;

	public void Setup()
	{
		_mainInputField.OnValueChangedHeight.Subscribe(delegate((float, float, bool) heights)
		{
			_offsetScrollFromDiaryChangedHeightCancelletionToken?.Cancel();
			_offsetScrollFromDiaryChangedHeightCancelletionToken?.Dispose();
			_offsetScrollFromDiaryChangedHeightCancelletionToken = new CancellationTokenSource();
			OffsetScrollFromDiaryChangedHeight(heights.Item2 - heights.Item1, _offsetScrollFromDiaryChangedHeightCancelletionToken.Token).Forget();
		}).AddTo(this);
	}

	public void SetText(string title, string main)
	{
		SetTitle(title);
		SetText(main);
	}

	public void SetTitle(string title)
	{
		_titleInputField.SetTextWithoutNotify(title);
	}

	public void SetText(string main)
	{
		_mainInputField.SetText(main);
	}

	public void StartTitleInput()
	{
		_titleInputField.ActivateInputField();
	}

	public void StartMainInput()
	{
		_mainInputField.ActivateInputField();
	}

	private async UniTask OffsetScrollFromDiaryChangedHeight(float changedHeight, CancellationToken cancellationToken)
	{
		await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken);
		await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken);
		_ = _mainInputScroll.content.rect.height;
		if (changedHeight > 0f)
		{
			Vector2 anchoredPosition = _mainInputScroll.content.anchoredPosition;
			anchoredPosition.y += changedHeight;
			_mainInputScroll.content.anchoredPosition = anchoredPosition;
		}
	}

	public void ResetScroll()
	{
		_mainInputScroll.verticalNormalizedPosition = 1f;
	}
}
