using UnityEngine;

namespace Bulbul.Mobile;

public class MusicPlayerVolumeIconView : MonoBehaviour
{
	[SerializeField]
	private GameObject _volumeNormalObj;

	[SerializeField]
	private GameObject _volumeMuteObj;

	public void ChangeMute(bool _isMute)
	{
		bool activeSelf = _volumeNormalObj.activeSelf;
		bool activeSelf2 = _volumeMuteObj.activeSelf;
		bool flag = !_isMute;
		bool flag2 = _isMute;
		if (activeSelf != flag)
		{
			_volumeNormalObj.SetActive(flag);
		}
		if (activeSelf2 != flag2)
		{
			_volumeMuteObj.SetActive(flag2);
		}
	}
}
