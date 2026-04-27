using UnityEngine;

public class SafeAreaAdjustor : MonoBehaviour
{
	private RectTransform obj_RectTrans;

	private Rect lastSafeArea;

	private void Awake()
	{
		obj_RectTrans = GetComponent<RectTransform>();
		Rect rect = (lastSafeArea = Screen.safeArea);
		Vector2 position = rect.position;
		Vector2 anchorMax = rect.position + rect.size;
		position.x /= Screen.width;
		position.y /= Screen.height;
		anchorMax.x /= Screen.width;
		anchorMax.y /= Screen.height;
		obj_RectTrans.anchorMin = position;
		obj_RectTrans.anchorMax = anchorMax;
	}

	private void Update()
	{
		Rect safeArea = Screen.safeArea;
		if (safeArea != lastSafeArea)
		{
			lastSafeArea = safeArea;
			Vector2 position = safeArea.position;
			Vector2 anchorMax = safeArea.position + safeArea.size;
			position.x /= Screen.width;
			position.y /= Screen.height;
			anchorMax.x /= Screen.width;
			anchorMax.y /= Screen.height;
			obj_RectTrans.anchorMin = position;
			obj_RectTrans.anchorMax = anchorMax;
		}
	}
}
