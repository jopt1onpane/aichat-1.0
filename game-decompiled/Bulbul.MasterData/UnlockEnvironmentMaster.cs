using UnityEngine;

namespace Bulbul.MasterData;

[CreateAssetMenu(fileName = "UnlockEnvironmentMaster", menuName = "ScriptableObject/UnlockEnvironmentMaster")]
public class UnlockEnvironmentMaster : ScriptableObject
{
	public UnlockEnvironmentData[] Items;

	public string CollectionName()
	{
		return "Items";
	}

	public void BeforeAssetDatabaseCreateAsset()
	{
	}
}
