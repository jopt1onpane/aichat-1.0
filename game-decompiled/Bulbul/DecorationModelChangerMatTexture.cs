using UnityEngine;

namespace Bulbul;

public class DecorationModelChangerMatTexture : DecorationModelChanger
{
	[SerializeField]
	private DecorationService.DecorationCategoryType _categoryType;

	[SerializeField]
	private Material _material;

	[SerializeField]
	private SerializedDictionaryEnum<DecorationService.DecorationSkinType, Texture> _skinTextures;

	public override DecorationService.DecorationCategoryType CategoryType => _categoryType;

	public override void Apply(DecorationService.DecorationModelType modelType, DecorationService.DecorationSkinType skinType)
	{
		if (_skinTextures.TryGetValue(skinType, out var value))
		{
			_material.mainTexture = value;
		}
	}
}
