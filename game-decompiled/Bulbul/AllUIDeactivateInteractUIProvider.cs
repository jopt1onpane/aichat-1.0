using UnityEngine;

namespace Bulbul;

public class AllUIDeactivateInteractUIProvider : MonoBehaviour, IAllUIDeactivateInteractUIProvider
{
	[SerializeField]
	private InteractableUI _allUIDeactivateInteractUI;

	public InteractableUI AllDeactivateInteractUI => _allUIDeactivateInteractUI;
}
