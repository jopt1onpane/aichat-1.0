using UnityEngine;

public class TapGuard : MonoBehaviour
{
	[SerializeField]
	private Canvas MobileCanves;

	private float screenHeight;

	private void Start()
	{
		screenHeight = 0f;
	}

	private void Update()
	{
		Vector2 sizeDelta = MobileCanves.GetComponent<RectTransform>().sizeDelta;
		if (screenHeight != sizeDelta.y)
		{
			screenHeight = sizeDelta.y;
			float num = screenHeight * -680f / 1920f;
			base.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0f, num, 0f);
			float y = screenHeight / 2f + num;
			base.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, y);
		}
	}
}
