using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class DecorationSkinMasterData
{
	public DecorationService.DecorationSkinType SkinType;

	public DecorationService.DecorationModelType ModelType;

	public Color IconColor1 = Color.white;

	public Color IconColor2 = Color.white;

	public Sprite Thumbnail;

	[Tooltip("カテゴリ内のデフォルト選択か")]
	public bool IsCategoryDefault;
}
