using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class LunaNewYear2026Parameter
{
	[SerializeField]
	[Header("次のレベルに必要な経験値")]
	private float nextLevelNecessaryExp;

	public float NextLevelNecessaryExp => nextLevelNecessaryExp;
}
