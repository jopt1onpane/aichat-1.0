using System;

namespace Bulbul;

[Serializable]
public struct TooltipData(string contentLocalizeID, ILocalizeConverter localizeConverter = null)
{
	public string ContentLocalizeID = contentLocalizeID;

	public ILocalizeConverter LocalizeConverter = localizeConverter;
}
