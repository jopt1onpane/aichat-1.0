using System;
using Com.ForbiddenByte.OSA.Core;
using UnityEngine;

namespace Bulbul.Mobile;

[Serializable]
public class TodoTaskListParam : BaseParams
{
	public RectTransform _todoTaskPullDown;

	public RectTransform _todoTaskCell;

	public RectTransform _taskListSeparator;

	public RectTransform _completeTaskPullDown;

	public RectTransform _completeTaskCell;

	public float _todoTaskPullDownHeight;

	public float _todoTaskCellHeight;

	public float _taskListSeparatorHeight;

	public float _completeTaskPullDownHeight;

	public float _completeTaskCellHeight;

	public override void InitIfNeeded(IOSA iAdapter)
	{
		base.InitIfNeeded(iAdapter);
	}
}
