using System;
using Com.ForbiddenByte.OSA.Core;
using UnityEngine;

namespace Bulbul.Mobile;

[Serializable]
public class CalendarContentParams : BaseParams
{
	public RectTransform CalendarAndWorkPrefab;

	public RectTransform CompletedTaskHeaderPrefab;

	public RectTransform CompletedTaskPrefab;

	public RectTransform DiaryPrefab;

	public float CalendarAndWorkPrefabHeight;

	public float CompletedTaskHeaderPrefabHeight;

	public float CompletedTaskPrefabHeight;

	public float DiaryPrefabHeight;

	public override void InitIfNeeded(IOSA iAdapter)
	{
		base.InitIfNeeded(iAdapter);
		AssertValidWidthHeight(CalendarAndWorkPrefab);
		AssertValidWidthHeight(CompletedTaskHeaderPrefab);
		AssertValidWidthHeight(CompletedTaskPrefab);
		AssertValidWidthHeight(DiaryPrefab);
	}
}
