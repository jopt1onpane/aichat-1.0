using UnityEngine;

namespace Bulbul;

public abstract class DecorationModelChanger : MonoBehaviour
{
	public abstract DecorationService.DecorationCategoryType CategoryType { get; }

	public abstract void Apply(DecorationService.DecorationModelType modelType, DecorationService.DecorationSkinType skinType);

	public virtual void DeactivateCategory()
	{
	}
}
