using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "WebEnvProfile", menuName = "WebEnvProfile", order = 0)]
public class WebEnvProfile : ScriptableObject
{
	[field: SerializeField]
	public string WebApiUrl { get; private set; }
}
