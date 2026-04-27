using UnityEngine;

namespace Bulbul.MasterData;

[CreateAssetMenu(fileName = "LocalizationMaster", menuName = "ScriptableObject/LocalizationMaster")]
public class LocalizationMaster : ScriptableObject
{
	public LocalizationData[] Items;

	public string CollectionName()
	{
		return "Items";
	}

	public void BeforeAssetDatabaseCreateAsset()
	{
		LocalizationData[] items = Items;
		foreach (LocalizationData obj in items)
		{
			obj.Ja = obj.Ja.Replace("\\n", "\n");
			obj.En = obj.En.Replace("\\n", "\n");
			obj.ZhHans = obj.ZhHans.Replace("\\n", "\n");
			obj.ZhHant = obj.ZhHant.Replace("\\n", "\n");
			obj.Pt = obj.Pt.Replace("\\n", "\n");
			obj.Ko = obj.Ko.Replace("\\n", "\n");
			obj.Ru = obj.Ru.Replace("\\n", "\n");
		}
	}
}
