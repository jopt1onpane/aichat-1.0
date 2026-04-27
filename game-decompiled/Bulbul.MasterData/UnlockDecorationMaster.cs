using UnityEngine;

namespace Bulbul.MasterData;

[CreateAssetMenu(fileName = "UnlockDecorationMaster", menuName = "ScriptableObject/UnlockDecorationMaster")]
public class UnlockDecorationMaster : ScriptableObject
{
	public UnlockDecorationData[] Items;

	public string CollectionName()
	{
		return "Items";
	}

	public void BeforeAssetDatabaseCreateAsset()
	{
	}
}
