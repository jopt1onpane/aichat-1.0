using System;

namespace Bulbul.Mobile;

public class CalendarContentsListBaseModel
{
	public float Height = -1f;

	public bool HasPendingSizeChange { get; set; } = true;

	public Type CachedType { get; private set; }

	public CalendarContentsListBaseModel()
	{
		CachedType = GetType();
	}
}
