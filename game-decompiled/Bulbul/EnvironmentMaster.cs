using System.Collections.Generic;
using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "EnvironmentMaster", menuName = "ScriptableObject/EnvironmentMaster")]
public class EnvironmentMaster : ScriptableObject, ISerializationCallbackReceiver
{
	[SerializeField]
	public EnvironmentMasterData[] Environments;

	private Dictionary<EnvironmentType, EnvironmentMasterData> _dic;

	public EnvironmentMasterData GetEnvironment(EnvironmentType environmentType)
	{
		return _dic.GetValueOrDefault(environmentType);
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		_dic = new Dictionary<EnvironmentType, EnvironmentMasterData>(Environments.Length);
		EnvironmentMasterData[] environments = Environments;
		foreach (EnvironmentMasterData environmentMasterData in environments)
		{
			_dic.Add(environmentMasterData.EnvironmentType, environmentMasterData);
		}
	}
}
