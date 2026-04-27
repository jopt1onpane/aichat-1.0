using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class InquiryIDView : MonoBehaviour
{
	private const string InquiryIDLocalizationKey = "ui_setting_inquiry_id_clickview";

	private const string InquiryIDClipboardLocalizationKey = "ui_setting_clipboard_copy";

	private const int InquiryIDDisplayLength = 10;

	private bool _isVisitable;

	private string _inquiryID = string.Empty;

	[SerializeField]
	private TMP_Text _inquiryIDText;

	[SerializeField]
	private TextLocalizationBehaviour _inquiryIDLocalizationText;

	[SerializeField]
	private Button _switchVisitableButton;

	[SerializeField]
	private Button _copyClipboardButton;

	[Header("ここから表示切替UI")]
	[SerializeField]
	private Image[] _switchVisitableImages;

	[SerializeField]
	private Sprite _notHoverVisitableIcon;

	[SerializeField]
	private Sprite _notVisitableIcon;

	[SerializeField]
	private Sprite _hoverVisitableIcon;

	[SerializeField]
	private Sprite _visitableIcon;

	[Inject]
	private SystemSeService systemSeService;

	[Inject]
	private SmallAnnounceService smallAnnounceService;

	public void Setup(string inquiryID)
	{
		_inquiryID = inquiryID.Substring(0, 10);
		Hide();
		ObservableSubscribeExtensions.Subscribe(_switchVisitableButton.OnClickAsObservable(), delegate
		{
			SwitchVisitable();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_copyClipboardButton.OnClickAsObservable(), delegate
		{
			CopyToClipboard();
		}).AddTo(this);
	}

	public void SwitchVisitable()
	{
		systemSeService.PlayClick();
		_isVisitable = !_isVisitable;
		if (_isVisitable)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void Show()
	{
		_inquiryIDText.text = _inquiryID;
		_switchVisitableImages[0].sprite = _visitableIcon;
		_switchVisitableImages[1].sprite = _hoverVisitableIcon;
		_isVisitable = true;
	}

	public void Hide()
	{
		_inquiryIDLocalizationText.Set("ui_setting_inquiry_id_clickview");
		_switchVisitableImages[0].sprite = _notVisitableIcon;
		_switchVisitableImages[1].sprite = _notHoverVisitableIcon;
		_isVisitable = false;
	}

	public void CopyToClipboard()
	{
		smallAnnounceService.Activate(7f, "ui_setting_clipboard_copy");
		systemSeService.PlayClick();
		GUIUtility.systemCopyBuffer = _inquiryID;
	}
}
