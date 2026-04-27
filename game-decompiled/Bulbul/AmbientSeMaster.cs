using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "AmbientSeMaster", menuName = "ScriptableObject/AmbientSeMaster")]
public class AmbientSeMaster : ScriptableObject
{
	public AmbientSeSoundMasterData[] AmbientSeSounds;
}
