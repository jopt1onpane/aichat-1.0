using System;
using UnityEngine;

namespace Bulbul;

public class CommonDialogOption
{
	public string TitleID;

	public Func<string, string> TitleSelector;

	public string BodyID;

	public Func<string, string> BodySelector;

	public Transform Parent;

	public bool UseCloseButton;

	public bool EnableCloseOnClickButton;

	public CommonButton[] Buttons;
}
