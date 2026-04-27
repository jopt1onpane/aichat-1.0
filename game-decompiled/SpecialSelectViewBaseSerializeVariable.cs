using Bulbul;
using UnityEngine;
using UnityEngine.UI;

public class SpecialSelectViewBaseSerializeVariable : MonoBehaviour
{
	[SerializeField]
	[Header("マウスホバー時画像アニメーション")]
	private InteractableUI _interactableUI;

	[SerializeField]
	[Header("マウスホバー時サイズアニメーション")]
	private HoldButtonAnimation _holdButtonAnim;

	[SerializeField]
	[Header("マウスホバー時フォント変更")]
	private FontMaterialChanger _fontMaterialChanger;

	[SerializeField]
	[Header("選択ボタン")]
	private Button _selectButton;

	[SerializeField]
	[Header("背景Image")]
	private Image _backImage;

	[SerializeField]
	[Header("アイコンImage:通常時")]
	private Image _iconNormalImage;

	[SerializeField]
	[Header("アイコンImage:使用時")]
	private Image _iconActiveImage;

	[SerializeField]
	[Header("タイトルテキスト")]
	private TextLocalizationBehaviour _localizationText;

	[SerializeField]
	[Header("背景画像:通常")]
	private Sprite _backgroundNormalSprite;

	[SerializeField]
	[Header("背景画像:グレーアウト")]
	private Sprite _backgroundGrayOutSprite;

	[SerializeField]
	[Header("進捗バー")]
	private GaugeView _progressBar;

	[SerializeField]
	[Header("既読アイコン:プレハブ")]
	private GameObject _readInteractableUIPrefab;

	[SerializeField]
	[Header("既読アイコン:親オブジェクト")]
	private Transform _readInteractableUIParent;

	public InteractableUI InteractableUI => _interactableUI;

	public HoldButtonAnimation HoldButtonAnim => _holdButtonAnim;

	public FontMaterialChanger FontMaterialChanger => _fontMaterialChanger;

	public Button SelectButton => _selectButton;

	public Image BackImage => _backImage;

	public Image IconNormalImage => _iconNormalImage;

	public Image IconActiveImage => _iconActiveImage;

	public TextLocalizationBehaviour LocalizationText => _localizationText;

	public Sprite BackgroundNormalSprite => _backgroundNormalSprite;

	public Sprite BackgroundGrayOutSprite => _backgroundGrayOutSprite;

	public GaugeView ProgressBar => _progressBar;

	public GameObject ReadInteractableUIPrefab => _readInteractableUIPrefab;

	public Transform ReadInteractableUIParent => _readInteractableUIParent;
}
