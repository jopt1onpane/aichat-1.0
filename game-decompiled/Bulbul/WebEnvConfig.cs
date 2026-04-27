using FastEnumUtility;
using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "WebEnvConfig", menuName = "WebEnvConfig", order = 0)]
public class WebEnvConfig : ScriptableObject
{
	[SerializeField]
	private WebEnvKind kind;

	[field: SerializeField]
	public WebEnvProfile ActiveProfile { get; private set; }

	public void Set(WebEnvKind kind)
	{
		this.kind = kind;
		ActiveProfile = Resources.Load<WebEnvProfile>("WebEnv/" + kind.ToName());
	}
}
