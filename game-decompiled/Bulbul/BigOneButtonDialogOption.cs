using System;
using UnityEngine;

namespace Bulbul;

public class BigOneButtonDialogOption
{
	public string TitleID;

	public Func<string, string> TitleSelector;

	public string BodyID;

	public Func<string, string> BodySelector;

	public Transform Parent;

	public bool EnableCloseOnClickButton;

	public CommonButton Button;
}
