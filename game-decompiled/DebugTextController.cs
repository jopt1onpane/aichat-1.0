using Bulbul;
using R3;
using TMPro;
using UnityEngine;
using VContainer;

public class DebugTextController : MonoBehaviour
{
	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private DebugService _debugService;

	[SerializeField]
	private TextMeshProUGUI _beforeStateText;

	[SerializeField]
	private TextMeshProUGUI _currentStateText;

	[SerializeField]
	private TextMeshProUGUI _heroineMotionIntegerText;

	[SerializeField]
	private TextMeshProUGUI _heroineMotionTypeText;

	[SerializeField]
	private TextMeshProUGUI _heroineUpdateStateText;

	[SerializeField]
	private Animator _heroineAnimator;

	private void Start()
	{
		SearchHeroineAnimator();
		ChangeShow(isUseDebug: false);
		_debugService.IsUseDebug.Subscribe(delegate(bool isUseDebug)
		{
			ChangeShow(isUseDebug);
		}).AddTo(this);
	}

	private void SearchHeroineAnimator()
	{
		if (!(_heroineAnimator != null))
		{
			HeroineService heroineService = Object.FindAnyObjectByType<HeroineService>();
			_heroineAnimator = heroineService.GetComponent<Animator>();
		}
	}

	private void LateUpdate()
	{
		_beforeStateText.text = _heroineService.GetBeforeAIState().ToString();
		_currentStateText.text = _heroineService.GetCurrentAIState().ToString();
		_heroineMotionIntegerText.text = _heroineAnimator.GetInteger("MotionType").ToString();
		_heroineMotionTypeText.text = ((HeroineService.AnimationType)_heroineAnimator.GetInteger("MotionType")/*cast due to .constrained prefix*/).ToString();
		_heroineUpdateStateText.text = _heroineService.GetHeroineUpdateStateType().ToString();
	}

	private void ChangeShow(bool isUseDebug)
	{
		base.gameObject.SetActive(isUseDebug);
	}
}
