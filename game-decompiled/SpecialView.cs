using Bulbul;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SpecialView : MonoBehaviour
{
	[Inject]
	private MasterDataLoader _masterDataLoader;

	[SerializeField]
	[Header("スペシャルリスト表示ボタン")]
	private Button _openSpecialListButton;

	[SerializeField]
	[Header("スペシャルリストUIのインターフェースを持つオブジェクト")]
	private GameObject _specialSelectListUIObj;

	[SerializeField]
	[Header("閉じるボタン")]
	private Button _closeButton;

	private ISpecialSelectListUI __specialSelectListUI;

	public Observable<Unit> OnClickOpenSpecialListButton => _openSpecialListButton.OnClickAsObservable();

	public Observable<Unit> OnClickCloseSpecialListButton => _closeButton.OnClickAsObservable();

	private ISpecialSelectListUI _specialSelectListUI
	{
		get
		{
			if (__specialSelectListUI == null)
			{
				__specialSelectListUI = _specialSelectListUIObj.GetComponent<ISpecialSelectListUI>();
			}
			return __specialSelectListUI;
		}
	}

	public void Setup()
	{
		_specialSelectListUI.Setup();
	}

	public void ActivateSpecialList()
	{
		_specialSelectListUI.Activate();
	}

	public void DeactivateSpecialList()
	{
		_specialSelectListUI.Deactivate();
	}
}
