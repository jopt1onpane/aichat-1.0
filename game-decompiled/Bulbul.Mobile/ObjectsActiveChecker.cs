using System.Collections.Generic;
using UnityEngine;

namespace Bulbul.Mobile;

public class ObjectsActiveChecker : MonoBehaviour
{
	[SerializeField]
	[Header("設定すると子のオブジェクトを探査して対象を含める（このオブジェクトは入らない）")]
	private GameObject _contentsRoot;

	[SerializeField]
	private GameObject[] _objs;

	private List<GameObject> _checkObjs = new List<GameObject>();

	private List<CanvasGroup> _checkCanvasGroups = new List<CanvasGroup>();

	public void Awake()
	{
		if (_contentsRoot != null)
		{
			int childCount = _contentsRoot.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				GameObject gameObject = _contentsRoot.transform.GetChild(i).gameObject;
				_checkObjs.Add(gameObject);
				gameObject.TryGetComponent<CanvasGroup>(out var component);
				_checkCanvasGroups.Add(component);
			}
		}
		GameObject[] objs = _objs;
		foreach (GameObject gameObject2 in objs)
		{
			_checkObjs.Add(gameObject2);
			gameObject2.TryGetComponent<CanvasGroup>(out var component2);
			_checkCanvasGroups.Add(component2);
		}
	}

	public bool CheckActive()
	{
		foreach (GameObject checkObj in _checkObjs)
		{
			if (checkObj.activeSelf)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckActiveAlpha()
	{
		foreach (CanvasGroup checkCanvasGroup in _checkCanvasGroups)
		{
			if (!(checkCanvasGroup == null) && checkCanvasGroup.alpha > 0f)
			{
				return true;
			}
		}
		return false;
	}
}
