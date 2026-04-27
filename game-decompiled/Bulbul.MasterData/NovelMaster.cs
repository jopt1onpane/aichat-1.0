using UnityEngine;

namespace Bulbul.MasterData;

[CreateAssetMenu(fileName = "NovelMaster", menuName = "ScriptableObject/NovelMaster")]
public class NovelMaster : ScriptableObject
{
	public NovelData[] Items;

	public string CollectionName()
	{
		return "Items";
	}

	public void BeforeAssetDatabaseCreateAsset()
	{
	}
}
