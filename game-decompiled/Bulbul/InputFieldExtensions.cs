using System;
using System.Linq;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;

namespace Bulbul;

public static class InputFieldExtensions
{
	public static void DisableIME(this TMP_InputField inputField)
	{
		ObservableSubscribeExtensions.Subscribe(inputField.OnUpdateSelectedAsObservable(), delegate
		{
			Input.imeCompositionMode = IMECompositionMode.Off;
		}).AddTo(inputField);
	}

	public static void SetupMultiLineSubmit(this TMP_InputField inputField)
	{
		inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
		TMP_InputField tMP_InputField = inputField;
		tMP_InputField.onValidateInput = (TMP_InputField.OnValidateInput)Delegate.Combine(tMP_InputField.onValidateInput, (TMP_InputField.OnValidateInput)delegate(string text, int index, char chara)
		{
			switch (chara)
			{
			case '\n':
				inputField.OnSubmit(null);
				inputField.OnDeselect(null);
				return '\0';
			case '\v':
				if (text.Count((char c) => c == '\n') + 1 >= inputField.lineLimit)
				{
					return '\0';
				}
				if (inputField.textComponent.textInfo.characterInfo[index].lineNumber >= inputField.lineLimit - 1)
				{
					return '\0';
				}
				return '\n';
			default:
				return chara;
			}
		});
	}
}
