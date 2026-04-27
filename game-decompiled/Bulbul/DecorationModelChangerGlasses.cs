using System;
using UnityEngine;

namespace Bulbul;

public class DecorationModelChangerGlasses : DecorationModelChanger
{
	[Serializable]
	private class SkinChangeData
	{
		public Mesh Mesh;

		public Mesh LensMesh;

		public Material Material;

		public Texture MainTex;
	}

	[SerializeField]
	private GameObject _modelRootObj;

	[SerializeField]
	private MeshFilter _frameMeshFilter;

	[SerializeField]
	private MeshRenderer _frameMeshRenderer;

	[SerializeField]
	private MeshFilter _lensMeshFilter;

	[SerializeField]
	private SerializedDictionaryEnum<DecorationService.DecorationSkinType, SkinChangeData> _skinDatas;

	public override DecorationService.DecorationCategoryType CategoryType => DecorationService.DecorationCategoryType.Glasses;

	public override void Apply(DecorationService.DecorationModelType modelType, DecorationService.DecorationSkinType skinType)
	{
		if (modelType == DecorationService.DecorationModelType.Glasses_None)
		{
			_modelRootObj.SetActive(value: false);
			return;
		}
		_modelRootObj.SetActive(value: true);
		if (_skinDatas.TryGetValue(skinType, out var value))
		{
			_frameMeshFilter.sharedMesh = value.Mesh;
			_frameMeshRenderer.sharedMaterial = value.Material;
			value.Material.mainTexture = value.MainTex;
			_lensMeshFilter.sharedMesh = value.LensMesh;
		}
	}
}
