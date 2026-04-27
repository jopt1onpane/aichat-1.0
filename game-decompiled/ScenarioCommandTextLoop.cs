using System.Collections.Generic;
using Bulbul.MasterData;
using UnityEngine;

public class ScenarioCommandTextLoop
{
	public enum LoopState
	{
		Idle,
		Looping,
		End
	}

	private LoopState _mainState;

	private List<NovelData> _loopTextDataList = new List<NovelData>();

	private RandomList _loopTextRandomList = new RandomList();

	private float _textPlayDelayMin;

	private float _textPlayDelayMax;

	private bool _isSelectedSelection;

	private NovelData _selectedNovelData = new NovelData();

	public LoopState MainState => _mainState;

	public bool IsSelectedSelection => _isSelectedSelection;

	public NovelData SelectedNovelData => _selectedNovelData;

	public void AddTextData(NovelData novelData)
	{
		_loopTextDataList.Add(novelData);
	}

	public void StartSetup(NovelData novelData)
	{
		_isSelectedSelection = false;
		_loopTextRandomList.Setup(0, _loopTextDataList.Count - 1);
		_textPlayDelayMin = float.Parse(novelData.Arg1);
		_textPlayDelayMax = novelData.Arg2;
		_mainState = LoopState.Looping;
	}

	public void OnSelectedSelection(NovelData selectionNovelData)
	{
		_isSelectedSelection = true;
		_selectedNovelData = selectionNovelData;
	}

	public void EndLoop()
	{
		_mainState = LoopState.End;
	}

	public NovelData GetNextLoopTextNovel()
	{
		return _loopTextDataList[_loopTextRandomList.GetNext()];
	}

	public void UseNextLoopTextNovel()
	{
		_loopTextRandomList.UseNext();
	}

	public float GetNextLoopTextRandomDelayTime()
	{
		return Random.Range(_textPlayDelayMin, _textPlayDelayMax);
	}

	public void EndTidying()
	{
		_isSelectedSelection = false;
		_loopTextDataList.Clear();
		_mainState = LoopState.Idle;
	}
}
