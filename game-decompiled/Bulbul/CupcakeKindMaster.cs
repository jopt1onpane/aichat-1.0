using System.Collections.Generic;
using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "CupcakeKindMaster", menuName = "ScriptableObject/CupcakeKindMaster")]
public class CupcakeKindMaster : ScriptableObject
{
	[SerializeField]
	private SerializedDictionaryEnum<HeroineCupcake.CupcakeKind, CupcakeKindRecord> _records;

	public CupcakeKindRecord GetCupcakeKindRecord(HeroineCupcake.CupcakeKind cupcakeKind)
	{
		return _records.GetValueOrDefault(cupcakeKind);
	}
}
