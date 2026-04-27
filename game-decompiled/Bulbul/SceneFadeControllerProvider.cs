using UnityEngine;

namespace Bulbul;

public class SceneFadeControllerProvider : MonoBehaviour
{
	[SerializeField]
	private FadeController _fadeController;

	public FadeController Controller => _fadeController;
}
