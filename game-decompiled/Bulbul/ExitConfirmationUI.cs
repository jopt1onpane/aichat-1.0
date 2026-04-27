using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class ExitConfirmationUI : MonoBehaviour
{
	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private FacilityAnimationBase _activateAnim;

	[SerializeField]
	private ClickOutsideDetector _clickOutsideDetector;

	[SerializeField]
	private Button _okButton;

	[SerializeField]
	private Button _cancelButton;

	public Observable<bool> OnDecide => (from _ in _okButton.OnClickAsObservable()
		select true).Merge(from _ in _cancelButton.OnClickAsObservable()
		select false);

	public Observable<Unit> OnClickOutside => _clickOutsideDetector.OnClickOutside;

	public bool IsActive { get; private set; }

	public virtual void Setup()
	{
		_activateAnim.Setup();
		base.gameObject.SetActive(value: false);
		IsActive = false;
	}

	public virtual void Activate()
	{
		IsActive = true;
		_activateAnim.Activate().Forget();
	}

	public virtual void Deactivate()
	{
		IsActive = false;
		_activateAnim.Deactivate().Forget();
	}

	public virtual void AddDontCloseOnClick(RectTransform trans)
	{
		_clickOutsideDetector.AddExcludeTransform(trans);
	}
}
