using UnityEngine;

namespace Bulbul;

public class DecorationModelChangerActive : DecorationModelChanger
{
	[SerializeField]
	private DecorationService.DecorationCategoryType _categoryType;

	[SerializeField]
	private SerializedDictionaryEnum<DecorationService.DecorationModelType, GameObject[]> _modelObjs;

	[SerializeField]
	private SerializedDictionaryEnum<DecorationService.DecorationSkinType, GameObject[]> _skinObjs;

	public override DecorationService.DecorationCategoryType CategoryType => _categoryType;

	public override void Apply(DecorationService.DecorationModelType modelType, DecorationService.DecorationSkinType skinType)
	{
		DeactivateAll();
		if (_modelObjs.TryGetValue(modelType, out var value))
		{
			GameObject[] array = value;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
		if (_skinObjs.TryGetValue(skinType, out var value2))
		{
			GameObject[] array = value2;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
	}

	public override void DeactivateCategory()
	{
		DeactivateAll();
	}

	private void DeactivateAll()
	{
		foreach (GameObject[] value in _modelObjs.Values)
		{
			for (int i = 0; i < value.Length; i++)
			{
				value[i].SetActive(value: false);
			}
		}
		foreach (GameObject[] value2 in _skinObjs.Values)
		{
			for (int i = 0; i < value2.Length; i++)
			{
				value2[i].SetActive(value: false);
			}
		}
	}
}
