using System;
using Com.ForbiddenByte.OSA.Core;
using UnityEngine;

namespace Bulbul.ScrollListSample;

[Serializable]
public class SampleListParams : BaseParams
{
	public RectTransform NormalPrefab;

	public RectTransform RichPrefab;
}
