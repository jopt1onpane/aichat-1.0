using System;
using UnityEngine;

namespace Bulbul;

public class DecorationModelChangerCommon : DecorationModelChanger
{
	[Serializable]
	private class SkinChangeData
	{
		public Mesh Mesh;

		public Material Material;

		public Texture MainTex;

		public Texture EmissionTex;
	}

	[SerializeField]
	private DecorationService.DecorationCategoryType _categoryType;

	[SerializeField]
	private MeshFilter[] _meshFilters;

	[SerializeField]
	private MeshRenderer[] _meshRenderers;

	[SerializeField]
	private SerializedDictionaryEnum<DecorationService.DecorationSkinType, SkinChangeData> _skinDatas;

	public override DecorationService.DecorationCategoryType CategoryType => _categoryType;

	public override void Apply(DecorationService.DecorationModelType modelType, DecorationService.DecorationSkinType skinType)
	{
		if (_skinDatas.TryGetValue(skinType, out var value))
		{
			MeshFilter[] meshFilters = _meshFilters;
			for (int i = 0; i < meshFilters.Length; i++)
			{
				meshFilters[i].sharedMesh = value.Mesh;
			}
			Material material = value.Material;
			MeshRenderer[] meshRenderers = _meshRenderers;
			for (int i = 0; i < meshRenderers.Length; i++)
			{
				meshRenderers[i].sharedMaterial = material;
			}
			material.mainTexture = value.MainTex;
			if (value.EmissionTex != null)
			{
				material.SetTexture("_EmissionMap", value.EmissionTex);
			}
		}
	}
}
