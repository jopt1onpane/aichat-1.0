using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

[Serializable]
[DoNotObfuscateClass]
public class RandomList
{
	[ES3Serializable]
	private List<int> _randomList = new List<int>();

	[ES3Serializable]
	private int _beforeLast = -1;

	private int _randomRangeStart;

	private int _randomRangeMax;

	private Action _onEndUseNextAction;

	public bool IsEmpty => _randomList.Count <= 0;

	public void Setup(int randomRangeMin, int randomRangeMax, Action onEndUseNext = null)
	{
		_randomRangeStart = randomRangeMin;
		_randomRangeMax = randomRangeMax - _randomRangeStart + _randomRangeStart;
		_onEndUseNextAction = onEndUseNext;
		ForceResetIfEmpty();
	}

	public void ForceResetIfEmpty()
	{
		if (_randomList.Count <= 0)
		{
			InitList();
		}
	}

	private void InitList()
	{
		for (int i = _randomRangeStart; i <= _randomRangeMax; i++)
		{
			_randomList.Add(i);
		}
		ShuffleUtils.Shuffle(_randomList);
		if (_randomList[0] == _beforeLast)
		{
			_randomList.RemoveAt(0);
			_randomList.Add(_beforeLast);
		}
		_beforeLast = _randomList[_randomList.Count - 1];
	}

	public int GetNext()
	{
		if (_randomList.Count <= 0)
		{
			InitList();
		}
		return _randomList[0];
	}

	public void UseNext()
	{
		_randomList.RemoveAt(0);
		_onEndUseNextAction?.Invoke();
	}

	public void RemoveFromList(int number)
	{
		if (_randomList.Count <= 0)
		{
			InitList();
		}
		_randomList.Remove(number);
	}
}
