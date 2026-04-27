using System.Collections.Generic;
using Com.ForbiddenByte.OSA.Core;

namespace Bulbul.Mobile;

public class PullDownAnimationStateHelper<Model, TargetHolder, BaseHolder> where BaseHolder : AbstractViewsHolder
{
	private List<Model> _keeps;

	private List<Model> _tempExclusions;

	public IReadOnlyCollection<Model> Keeps => _keeps;

	public bool IsKeep => _keeps.Count != 0;

	public int Count => _keeps.Count;

	public PullDownAnimationStateHelper(int capacity = 30)
	{
		_keeps = new List<Model>(capacity);
		_tempExclusions = new List<Model>(capacity);
	}

	public void Clear()
	{
		_keeps.Clear();
		_tempExclusions.Clear();
	}

	public (int, int) AddTempExclusions(List<BaseHolder> visibleBaseHolders, OSAListDataHelper<Model> models, int bottomIdx)
	{
		int num = -1;
		foreach (BaseHolder visibleBaseHolder in visibleBaseHolders)
		{
			if (num < visibleBaseHolder.ItemIndex)
			{
				num = visibleBaseHolder.ItemIndex;
			}
		}
		num++;
		for (int i = num; i <= bottomIdx; i++)
		{
			_tempExclusions.Add(models[i]);
		}
		return (num, bottomIdx + 1 - num);
	}

	public void MargeTempExclusionsIntoKeeps()
	{
		_keeps.AddRange(_tempExclusions);
		_tempExclusions.Clear();
	}

	public Model TakeModelOut(int idx)
	{
		Model result = _keeps[idx];
		_keeps.RemoveAt(idx);
		return result;
	}

	public Model TakeModelOutAtLast()
	{
		return TakeModelOut(_keeps.Count - 1);
	}

	public void Add(Model model)
	{
		_keeps.Add(model);
	}

	public void InsertModelsKeepsToArg(OSAListDataHelper<Model> models, int idx)
	{
		models.InsertItems(idx, _keeps);
	}
}
