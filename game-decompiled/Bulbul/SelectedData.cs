using System.Collections.Generic;

namespace Bulbul;

public class SelectedData
{
	private List<float> _useSelectionIDList = new List<float>();

	public void Reset()
	{
		_useSelectionIDList.Clear();
	}

	public void UseSelection(float id)
	{
		if (id != 0f && !_useSelectionIDList.Contains(id))
		{
			_useSelectionIDList.Add(id);
		}
	}

	public bool IsUsedID(float id)
	{
		if (_useSelectionIDList.Contains(id))
		{
			return true;
		}
		return false;
	}
}
