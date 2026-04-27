using UnityEngine;

namespace Bulbul.MasterData;

[CreateAssetMenu(fileName = "NovelMaster", menuName = "ScriptableObject/ScenarioGroupMaster")]
public class ScenarioGroupMaster : ScriptableObject
{
	public ScenarioGroupData[] Items;

	public string CollectionName()
	{
		return "Items";
	}

	public void BeforeAssetDatabaseCreateAsset()
	{
	}
}
