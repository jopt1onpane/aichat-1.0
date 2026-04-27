using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bulbul;

public class DecorationModelChangerHeadphone : DecorationModelChanger
{
	[Serializable]
	private class ModelChangeData
	{
		public GameObject ModelObj;

		public Renderer Renderer;
	}

	[Serializable]
	private class SkinChangeData
	{
		public Material Material;

		public Texture MainTex;
	}

	[SerializeField]
	private SerializedDictionaryEnum<DecorationService.DecorationModelType, ModelChangeData> _modelDatas;

	[SerializeField]
	private SerializedDictionaryEnum<DecorationService.DecorationSkinType, SkinChangeData> _skinDatas;

	private readonly Dictionary<DecorationService.DecorationSkinType, Material> _skinMaterials = new Dictionary<DecorationService.DecorationSkinType, Material>();

	public override DecorationService.DecorationCategoryType CategoryType => DecorationService.DecorationCategoryType.Headphone;

	public override void Apply(DecorationService.DecorationModelType modelType, DecorationService.DecorationSkinType skinType)
	{
		Renderer renderer = null;
		foreach (KeyValuePair<DecorationService.DecorationModelType, ModelChangeData> modelData in _modelDatas)
		{
			modelData.Deconstruct(out var key, out var value);
			DecorationService.DecorationModelType num = key;
			ModelChangeData modelChangeData = value;
			bool flag = num == modelType;
			modelChangeData.ModelObj.SetActive(flag);
			if (flag)
			{
				renderer = modelChangeData.Renderer;
			}
		}
		SkinChangeData value2;
		if (renderer == null)
		{
			Debug.LogError($"Renderer not found for model type: {modelType}");
		}
		else if (_skinDatas.TryGetValue(skinType, out value2))
		{
			if (!_skinMaterials.TryGetValue(skinType, out var value3))
			{
				value3 = new Material(value2.Material);
				_skinMaterials[skinType] = value3;
			}
			value3.mainTexture = value2.MainTex;
			renderer.material = value3;
		}
	}

	private void OnDestroy()
	{
		foreach (Material value in _skinMaterials.Values)
		{
			if (value != null)
			{
				UnityEngine.Object.Destroy(value);
			}
		}
		_skinMaterials.Clear();
	}
}
