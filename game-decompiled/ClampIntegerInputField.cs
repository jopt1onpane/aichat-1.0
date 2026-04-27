using R3;
using TMPro;
using UnityEngine;

public class ClampIntegerInputField : MonoBehaviour
{
	[SerializeField]
	[Header("インプットフィールド")]
	private TMP_InputField _inputField;

	[SerializeField]
	[Header("最低値")]
	private int _min;

	[SerializeField]
	[Header("最大値")]
	private int _max;

	private readonly Subject<int> _onEndEdit = new Subject<int>();

	public Observable<int> OnEditEnd => _onEndEdit;

	public void Setup(int initValue = 0)
	{
		_inputField.text = initValue.ToString();
		_inputField.OnEndEditAsObservable().Subscribe(delegate(string text)
		{
			int value = int.Parse(text);
			value = Mathf.Clamp(value, _min, _max);
			_inputField.text = value.ToString();
			_onEndEdit.OnNext(value);
		}).AddTo(this);
	}

	public void SetText(string text)
	{
		_inputField.text = text;
	}
}
