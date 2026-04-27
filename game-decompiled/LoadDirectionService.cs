using Bulbul;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadDirectionService : MonoBehaviour
{
	public enum DirectionType
	{
		Normal,
		Error
	}

	private const string DirectionStartTrigger = "StartSplash";

	private const string DirectionFinishTrigger = "SplashFinish";

	private const string DirectionFinishAnimName = "anim_ui_loading_finish";

	private const string DirectionFadeInTrigger = "SplashFadeIn";

	private const string DirectionFadeInAnimName = "anim_ui_loading_fadein";

	private const string DirectionErrorFinishTrigger = "SplashErrorFinish";

	private const string DirectionErrorFinishAnimName = "anim_ui_loading_finish_error";

	private const string DirectionErrorFadeInTrigger = "SplashErrorFadeInFinish";

	private const string DirectionErrorFadeInAnimName = "anim_ui_loading_fadein_error";

	private const string DirectionGameEndTrigger = "GameEnd";

	private const string DirectionGameEndInAnimName = "anim_ui_game_end";

	private const string DirectionGameFinishEndCreditsTrigger = "FinishEndCredits";

	[SerializeField]
	private Animator directionAnimatorStandalone;

	[SerializeField]
	private Animator directionAnimatorMobile;

	[SerializeField]
	private Canvas canvasStandalone;

	[SerializeField]
	private Canvas canvasMobile;

	private Animator _directionAnimator;

	private void Awake()
	{
		canvasStandalone.gameObject.SetActive(DevicePlatform.Steam.IsPC());
		canvasMobile.gameObject.SetActive(DevicePlatform.Steam.IsMobile());
		_directionAnimator = (DevicePlatform.Steam.IsMobile() ? directionAnimatorMobile : directionAnimatorStandalone);
	}

	public void StartDirection(bool landscape)
	{
		SwitchAnimator(landscape);
		_directionAnimator.SetTrigger("StartSplash");
	}

	public void FinishDirection(DirectionType type, bool landscape)
	{
		SwitchAnimator(landscape);
		string trigger = ((type == DirectionType.Normal) ? "SplashFinish" : "SplashErrorFinish");
		_directionAnimator.SetTrigger(trigger);
	}

	public bool IsFinishDirection(DirectionType type)
	{
		string animationName = ((type == DirectionType.Normal) ? "anim_ui_loading_finish" : "anim_ui_loading_finish_error");
		return MyAnimatorUtil.IsEndAnimation(_directionAnimator, animationName);
	}

	public void FadeInGame(DirectionType type, bool landscape)
	{
		SwitchAnimator(landscape);
		string trigger = ((type == DirectionType.Normal) ? "SplashFadeIn" : "SplashErrorFadeInFinish");
		_directionAnimator.SetTrigger(trigger);
	}

	public bool IsFinishFadeIn(DirectionType type)
	{
		string animationName = ((type == DirectionType.Normal) ? "anim_ui_loading_fadein" : "anim_ui_loading_fadein_error");
		return MyAnimatorUtil.IsEndAnimation(_directionAnimator, animationName);
	}

	public void GameEndDirection(bool landscape)
	{
		SwitchAnimator(landscape);
		_directionAnimator.SetTrigger("GameEnd");
	}

	public async UniTask FinishEndCredits(bool landscape)
	{
		SwitchAnimator(landscape);
		_directionAnimator.SetTrigger("FinishEndCredits");
		await UniTask.WaitForSeconds(4f);
		FinishDirection(DirectionType.Normal, landscape);
		await UniTask.WaitUntil(() => IsFinishDirection(DirectionType.Normal));
	}

	public bool IsFinishGameEndDirection()
	{
		return MyAnimatorUtil.IsEndAnimation(_directionAnimator, "anim_ui_game_end");
	}

	private void SyncAnimator(Animator to, Animator from)
	{
		AnimatorStateInfo currentAnimatorStateInfo = from.GetCurrentAnimatorStateInfo(0);
		to.Play(currentAnimatorStateInfo.shortNameHash, 0, currentAnimatorStateInfo.normalizedTime);
	}

	private void SwitchAnimator(bool landscape)
	{
		Animator animator = directionAnimatorMobile;
		if (landscape)
		{
			animator = directionAnimatorStandalone;
		}
		if (!(_directionAnimator == animator))
		{
			canvasStandalone.gameObject.SetActive(value: true);
			canvasMobile.gameObject.SetActive(value: true);
			SyncAnimator(animator, _directionAnimator);
			_directionAnimator = animator;
			canvasStandalone.gameObject.SetActive(landscape);
			canvasMobile.gameObject.SetActive(!landscape);
		}
	}

	public void DebugImmediateStartFadeIn()
	{
		_directionAnimator.Play("anim_ui_loading_fadein", 0, 0f);
	}
}
