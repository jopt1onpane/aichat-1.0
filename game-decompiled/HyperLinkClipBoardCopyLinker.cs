using Bulbul;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

public class HyperLinkClipBoardCopyLinker : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private const string CopyClipboardLocalizationKey = "ui_setting_clipboard_copy";

	[SerializeField]
	private TMP_Text hyperText;

	[Inject]
	private SystemSeService systemSeService;

	[Inject]
	private SmallAnnounceService smallAnnounceService;

	public void OnPointerClick(PointerEventData eventData)
	{
		Vector2 vector = Input.mousePosition;
		Canvas canvas = hyperText.canvas;
		Camera camera = ((canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera);
		int num = TMP_TextUtilities.FindIntersectingLink(hyperText, vector, camera);
		if (num != -1)
		{
			smallAnnounceService.Activate(7f, "ui_setting_clipboard_copy");
			systemSeService.PlayClick();
			TMP_LinkInfo tMP_LinkInfo = hyperText.textInfo.linkInfo[num];
			GUIUtility.systemCopyBuffer = tMP_LinkInfo.GetLinkID();
		}
	}
}
