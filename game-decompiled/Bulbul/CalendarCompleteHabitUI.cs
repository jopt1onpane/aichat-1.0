using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul;

public class CalendarCompleteHabitUI : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private TMP_InputField _titleInputField;

	[SerializeField]
	private Button _deleteButton;

	[SerializeField]
	private TMP_Text _dateText;

	private DisposableBag _disposableBag;

	public void Setup(ReadOnlyReactiveProperty<string> title, DateTime date, Action onClickDelete, Action<string> onTitleEndEdit)
	{
		_disposableBag.Clear();
		title.Subscribe(delegate(string val)
		{
			_titleInputField.SetTextWithoutNotify(val);
		}).AddTo(ref _disposableBag);
		_titleInputField.SetupMultiLineSubmit();
		_titleInputField.OnEndEditAsObservable().Subscribe(delegate(string val)
		{
			onTitleEndEdit?.Invoke(val);
		}).AddTo(ref _disposableBag);
		_dateText.text = $"{date:MM/dd}";
		_deleteButton.OnClickAsObservable().Subscribe(onClickDelete).AddTo(ref _disposableBag);
		_deleteButton.gameObject.SetActive(value: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_deleteButton.gameObject.SetActive(value: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_deleteButton.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		_disposableBag.Dispose();
	}
}
