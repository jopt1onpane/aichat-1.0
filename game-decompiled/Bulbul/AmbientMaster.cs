using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "AmbientMaster", menuName = "ScriptableObject/AmbientMaster")]
public class AmbientMaster : ScriptableObject
{
	public AmbientSoundMasterData[] AmbientSounds;
}
