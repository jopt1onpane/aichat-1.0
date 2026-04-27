using System;

namespace Bulbul;

public readonly struct CommonButton(string textID, CommonButtonStyle style = CommonButtonStyle.Normal, SystemSeType seType = SystemSeType.Click, Func<string, string> textSelector = null)
{
	public readonly string TextID = textID;

	public readonly CommonButtonStyle Style = style;

	public readonly Func<string, string> TextSelector = textSelector;

	public readonly SystemSeType SeType = seType;
}
