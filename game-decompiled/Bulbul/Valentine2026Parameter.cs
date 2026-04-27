using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class Valentine2026Parameter
{
	[SerializeField]
	[Header("次のレベルに必要な経験値")]
	private float nextLevelNecessaryExp;

	public float NextLevelNecessaryExp => nextLevelNecessaryExp;
}
