using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class CupcakeKindRecord
{
	[SerializeField]
	[Header("レアリティ")]
	public HeroineCupcake.CupcakeRarity Rarity;

	[SerializeField]
	[Header("カップ：メッシュ")]
	public Mesh BakingCupMesh;

	[SerializeField]
	[Header("カップ：マテリアル")]
	public Material BakingCupMaterial;

	[SerializeField]
	[Header("ケーキ01：メッシュ")]
	public Mesh Cake01Mesh;

	[SerializeField]
	[Header("ケーキ01：マテリアル")]
	public Material Cake01Material;

	[SerializeField]
	[Header("ケーキ02：メッシュ")]
	public Mesh Cake02Mesh;

	[SerializeField]
	[Header("ケーキ02：マテリアル")]
	public Material Cake02Material;
}
