using System;
using Bulbul;
using UnityEngine;

public class HeroineUseObjectController : MonoBehaviour
{
	public enum DeskObjectAnimationType
	{
		None,
		FromBookToPc,
		FromReportToPc,
		FromPcToBook,
		FromReportToBook,
		FromPcToReport,
		FromBookToReport,
		PutWorkBook,
		TakeWorkBook
	}

	public enum ChairAnimationType
	{
		Idle = 0,
		PlayFirst = 1,
		PlaySecond = 2,
		PlayEnd = 3,
		SubBase_006 = 4,
		PlayOpenWindow = 5,
		PlayCloseWindow = 6,
		StartUseCoffeeMaker = 7,
		LoopUseCoffeeMaker = 8,
		EndUseCoffeeMaker = 9,
		StartBreakMovie = 600,
		LoopBreakMovie = 601,
		EndBreakMovie = 602,
		StartBreakReadBook = 650,
		EndBreakReadBook = 651,
		PickupDroppedPen = 150,
		Greeting_Wave_1 = 500,
		LeaveChair_Normal_Go = 5000,
		LeaveChair_Normal_Comeback = 5001,
		LeaveChair_Cupcake_Comeback = 5020,
		LeaveChair_Cupcake_Show_Loop = 5021,
		LeaveChair_Cupcake_Eat_Start_1 = 5022,
		LeaveChair_Cupcake_Eat_Loop_1 = 5023,
		LeaveChair_Cupcake_Eat_Start_2 = 5024,
		LeaveChair_Cupcake_Eat_Loop_2 = 5025,
		LeaveChair_Cupcake_Eat_End = 5026,
		LeaveChair_Sofa_Go = 5100,
		LeaveChair_Sofa_Comeback = 5101
	}

	public enum PenAnimationType
	{
		StartSpinning,
		LoopSpinning,
		EndSpinning,
		PickupDroppedPen
	}

	private const string MotionType = "MotionType";

	private const string DeskObjectAnimationKey = "MotionType";

	private const string BreakBookNextPageAnimName = "NextPage";

	private const string BreakBookBackPageAnimName = "BackPage";

	private const string WorkBookNextPageTriggerKey = "NextPage";

	private const string ChairIdleAnimationName = "Chair_Idle";

	private const string PenIdleAnimationName = "Pen_Idle";

	[Header("ヒロインの手に持っているオブジェクト")]
	[SerializeField]
	[Header("キャラクターの手のボーンに紐づくペン")]
	private MeshRenderer _handPen;

	[SerializeField]
	[Header("ペン回し用のペン")]
	private MeshRenderer _handPenForSpin;

	[SerializeField]
	[Header("部屋にあるペン")]
	private MeshRenderer _roomPen;

	[SerializeField]
	[Header("手に持っているノート")]
	private GameObject _handNote;

	[SerializeField]
	[Header("手に持っているコップ")]
	private GameObject _handCop;

	[SerializeField]
	[Header("部屋にあるコップ")]
	private GameObject _roomCop;

	[SerializeField]
	[Header("コーヒーメーカーのコップ")]
	private GameObject _coffeeMakerCop;

	[Header("休憩用の本の操作")]
	[SerializeField]
	private Animator _breakBookAnimator;

	[Header("作業用の本の操作")]
	[SerializeField]
	private Animator _workBookAnimator;

	[Header("デスク上のオブジェクトの操作")]
	[SerializeField]
	private Animator _deskObjectAnimator;

	[Header("イスの操作")]
	[SerializeField]
	private Animator _chairAnimator;

	[Header("イスの操作(モバイル)")]
	[SerializeField]
	private Animator _chairAnimatorMobile;

	[Header("ペンの操作")]
	[SerializeField]
	private Animator _penAnimator;

	[Header("カップケーキ")]
	[SerializeField]
	private HeroineCupcake _cupcake;

	[Header("窓の操作")]
	[SerializeField]
	private WindowController _windowController;

	[Header("コーヒーメーカーの操作")]
	[SerializeField]
	private ParticleSystem _coffeeDropParticle;

	[Header("飲み物")]
	[SerializeField]
	private HeroineDrinks _drinks;

	[Header("くまのレストランコラボ用の本")]
	[SerializeField]
	private BearsRestaurantBookForStory _bearsRestaurantBook;

	private DeskType _deskType = DeskType.Pc;

	public DeskObjectAnimationType _currentAnimationType = DeskObjectAnimationType.FromBookToPc;

	private Animator _currentChairAnimator;

	public DeskType DeskType => _deskType;

	public HeroineDrinks Drinks => _drinks;

	public void Setup(HeroineService heroineService)
	{
		_currentChairAnimator = (DevicePlatform.Steam.IsMobile() ? _chairAnimatorMobile : _chairAnimator);
		DeactivateAllObject();
		_cupcake.Setup();
		_drinks.Setup(heroineService);
		_windowController.Setup();
	}

	public void StoryReady()
	{
		DeactivateDropCoffee();
		ImmediateTidyingWindowAnimation();
		DeactivateAllObject();
		ImmediateTidyingChairAnimation();
		_cupcake.StoryReady();
	}

	public void StoryTidying()
	{
		ImmediateTidyingChairAnimation();
		DeactivateAllObject();
	}

	public void DeactivateAllObject()
	{
		DeactivatePen();
		DeactivateBook();
		DeactivateCop();
		_bearsRestaurantBook.Deactivate();
	}

	public void ActivatePen()
	{
		_handPen.enabled = true;
		_handPenForSpin.enabled = false;
		_roomPen.enabled = false;
	}

	public void ActivatePenForSpin()
	{
		_handPen.enabled = false;
		_handPenForSpin.enabled = true;
		_roomPen.enabled = false;
	}

	public void DeactivatePen()
	{
		_handPen.enabled = false;
		_handPenForSpin.enabled = false;
		_roomPen.enabled = true;
	}

	public void ActivateCop()
	{
		_roomCop.gameObject.SetActive(value: false);
		_handCop.gameObject.SetActive(value: true);
		_drinks.SetCupSmokeToTarget(_handCop.transform);
	}

	public void DeactivateCop()
	{
		_handCop.gameObject.SetActive(value: false);
		_coffeeMakerCop.gameObject.SetActive(value: false);
		_roomCop.gameObject.SetActive(value: true);
		_drinks.SetCupSmokeToTarget(_roomCop.transform);
	}

	public void PutCopOnCoffeeMaker()
	{
		_handCop.gameObject.SetActive(value: false);
		_coffeeMakerCop.gameObject.SetActive(value: true);
		_drinks.SetCupSmokeToTarget(_coffeeMakerCop.transform);
	}

	public void TakeCopFromCoffeeMaker()
	{
		_coffeeMakerCop.gameObject.SetActive(value: false);
		_handCop.gameObject.SetActive(value: true);
		_drinks.SetCupSmokeToTarget(_handCop.transform);
	}

	public void PourDrinksFromCoffeeMaker()
	{
		_drinks.PourDrinks();
	}

	public void ActivateBook()
	{
		_handNote.gameObject.SetActive(value: true);
	}

	public void DeactivateBook()
	{
		_handNote.gameObject.SetActive(value: false);
	}

	public void BreakBookNextPage()
	{
		_breakBookAnimator.Play("NextPage", 0, 0f);
	}

	public void BreakBookBackPage()
	{
		_breakBookAnimator.Play("BackPage", 0, 0f);
	}

	public void WorkBookNextPage()
	{
		_workBookAnimator.SetTrigger("NextPage");
	}

	private void ChangeDeskUseObjectAnimation(DeskObjectAnimationType type)
	{
		_deskObjectAnimator.SetInteger("MotionType", (int)type);
	}

	public void ImmediateChangeDeskUseObjectAnimation(HeroineService.AnimationType animType)
	{
		switch (animType)
		{
		case HeroineService.AnimationType.WorkBase003:
		case HeroineService.AnimationType.WorkBase003_SmallThinking:
		case HeroineService.AnimationType.WorkBase003_BigThinking:
			ActivatePen();
			break;
		case HeroineService.AnimationType.BreakBase002:
		case HeroineService.AnimationType.BreakBase002_NextPage:
		case HeroineService.AnimationType.BreakBase002_PreviousPage:
		case HeroineService.AnimationType.BreakBase002_Interest:
			ActivateBook();
			break;
		case HeroineService.AnimationType.BreakBase004:
		case HeroineService.AnimationType.BreakBase004_DrinkTea:
		case HeroineService.AnimationType.BreakBase004_Stretch:
		case HeroineService.AnimationType.BreakBase004_DrinkHot:
		case HeroineService.AnimationType.BreakBase004_CoolingDrinkTea:
			ActivateCop();
			break;
		}
	}

	public void ImmediateChangeDeskType(DeskType deskType = DeskType.None)
	{
		DeskObjectAnimationType currentAnimationType = _currentAnimationType;
		switch (deskType)
		{
		case DeskType.Pc:
			FromBookToPc();
			currentAnimationType = DeskObjectAnimationType.FromBookToPc;
			break;
		case DeskType.Book:
			FromPcToBook();
			currentAnimationType = DeskObjectAnimationType.FromPcToBook;
			break;
		case DeskType.Report:
			FromPcToReport();
			currentAnimationType = DeskObjectAnimationType.FromPcToReport;
			break;
		default:
			currentAnimationType = _currentAnimationType;
			break;
		}
		_deskObjectAnimator.Play(currentAnimationType.ToString(), 0, 1f);
	}

	public void ImmediateCurrentChangeDesk()
	{
		_deskObjectAnimator.Play(_currentAnimationType.ToString(), 0, 1f);
	}

	public void TakeWorkBook()
	{
		ChangeDeskUseObjectAnimation(DeskObjectAnimationType.TakeWorkBook);
	}

	public void PutWorkBook()
	{
		ChangeDeskUseObjectAnimation(DeskObjectAnimationType.PutWorkBook);
	}

	public void FromBookToPc()
	{
		ChangeDeskUseObjectAnimation(DeskObjectAnimationType.FromBookToPc);
		_currentAnimationType = DeskObjectAnimationType.FromBookToPc;
		_deskType = DeskType.Pc;
	}

	public void FromReportToPc()
	{
		ChangeDeskUseObjectAnimation(DeskObjectAnimationType.FromReportToPc);
		_currentAnimationType = DeskObjectAnimationType.FromReportToPc;
		_deskType = DeskType.Pc;
	}

	public void FromPcToBook()
	{
		ChangeDeskUseObjectAnimation(DeskObjectAnimationType.FromPcToBook);
		_currentAnimationType = DeskObjectAnimationType.FromPcToBook;
		_deskType = DeskType.Book;
	}

	public void FromReportToBook()
	{
		ChangeDeskUseObjectAnimation(DeskObjectAnimationType.FromReportToBook);
		_currentAnimationType = DeskObjectAnimationType.FromReportToBook;
		_deskType = DeskType.Book;
	}

	public void FromPcToReport()
	{
		ChangeDeskUseObjectAnimation(DeskObjectAnimationType.FromPcToReport);
		_currentAnimationType = DeskObjectAnimationType.FromPcToReport;
		_deskType = DeskType.Report;
	}

	public void FromBookToReport()
	{
		ChangeDeskUseObjectAnimation(DeskObjectAnimationType.FromBookToReport);
		_currentAnimationType = DeskObjectAnimationType.FromBookToReport;
		_deskType = DeskType.Report;
	}

	private void ChangeChairAnimation(ChairAnimationType type)
	{
		_currentChairAnimator.ResetTrigger(ChairAnimationType.PlayFirst.ToString());
		_currentChairAnimator.ResetTrigger(ChairAnimationType.PlaySecond.ToString());
		_currentChairAnimator.ResetTrigger(ChairAnimationType.PlayEnd.ToString());
		_currentChairAnimator.ResetTrigger(ChairAnimationType.PlayOpenWindow.ToString());
		_currentChairAnimator.ResetTrigger(ChairAnimationType.PlayCloseWindow.ToString());
		switch (type)
		{
		case ChairAnimationType.SubBase_006:
		case ChairAnimationType.StartUseCoffeeMaker:
		case ChairAnimationType.LoopUseCoffeeMaker:
		case ChairAnimationType.EndUseCoffeeMaker:
		case ChairAnimationType.PickupDroppedPen:
		case ChairAnimationType.Greeting_Wave_1:
		case ChairAnimationType.StartBreakMovie:
		case ChairAnimationType.LoopBreakMovie:
		case ChairAnimationType.EndBreakMovie:
		case ChairAnimationType.StartBreakReadBook:
		case ChairAnimationType.EndBreakReadBook:
		case ChairAnimationType.LeaveChair_Normal_Go:
		case ChairAnimationType.LeaveChair_Normal_Comeback:
		case ChairAnimationType.LeaveChair_Cupcake_Comeback:
		case ChairAnimationType.LeaveChair_Cupcake_Show_Loop:
		case ChairAnimationType.LeaveChair_Cupcake_Eat_Start_1:
		case ChairAnimationType.LeaveChair_Cupcake_Eat_Loop_1:
		case ChairAnimationType.LeaveChair_Cupcake_Eat_Start_2:
		case ChairAnimationType.LeaveChair_Cupcake_Eat_Loop_2:
		case ChairAnimationType.LeaveChair_Cupcake_Eat_End:
		case ChairAnimationType.LeaveChair_Sofa_Go:
		case ChairAnimationType.LeaveChair_Sofa_Comeback:
			_currentChairAnimator.Play(type.ToString(), 0, 0f);
			break;
		case ChairAnimationType.Idle:
			_currentChairAnimator.SetInteger("MotionType", (int)type);
			break;
		default:
			_currentChairAnimator.SetTrigger(type.ToString());
			break;
		}
	}

	public void PlayChairAnimationIdle()
	{
		ChangeChairAnimation(ChairAnimationType.Idle);
	}

	public void PlayChairAnimationFirst()
	{
		ChangeChairAnimation(ChairAnimationType.PlayFirst);
	}

	public void PlayChairAnimationSecond()
	{
		ChangeChairAnimation(ChairAnimationType.PlaySecond);
	}

	public void PlayChairAnimationSubBase006()
	{
		ChangeChairAnimation(ChairAnimationType.SubBase_006);
	}

	public void PlayChairAnimationStartBreakMovie()
	{
		ChangeChairAnimation(ChairAnimationType.StartBreakMovie);
	}

	public void PlayChairAnimationLoopBreakMovie()
	{
		ChangeChairAnimation(ChairAnimationType.LoopBreakMovie);
	}

	public void PlayChairAnimationEndBreakMovie()
	{
		ChangeChairAnimation(ChairAnimationType.EndBreakMovie);
	}

	public void PlayChairAnimationStartBreakReadBook()
	{
		ChangeChairAnimation(ChairAnimationType.StartBreakReadBook);
	}

	public void PlayChairAnimationEndBreakReadBook()
	{
		ChangeChairAnimation(ChairAnimationType.EndBreakReadBook);
	}

	public void PlayChairAnimationPickupDroppedPen()
	{
		ChangeChairAnimation(ChairAnimationType.PickupDroppedPen);
	}

	public void PlayChairAnimationStartSitFromOutSide()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Normal_Go);
	}

	private void ChangePenAnimation(PenAnimationType type)
	{
		_penAnimator.Play(type.ToString(), 0, 0f);
	}

	public void PlayPenAnimationStartSpinning()
	{
		ChangePenAnimation(PenAnimationType.StartSpinning);
	}

	public void PlayPenAnimationLoopSpinning()
	{
		ChangePenAnimation(PenAnimationType.LoopSpinning);
	}

	public void PlayPenAnimationEndSpinning()
	{
		ChangePenAnimation(PenAnimationType.EndSpinning);
	}

	public void PlayPenAnimationPickupDroppedPen()
	{
		ChangePenAnimation(PenAnimationType.PickupDroppedPen);
	}

	public void EatCake1()
	{
		_cupcake.EatCake1();
	}

	public void EatCake2()
	{
		_cupcake.EatCake2();
	}

	public void CrashCup()
	{
		_cupcake.CrashCup();
	}

	public void PlayChairAnimationOpenWindow()
	{
		ChangeChairAnimation(ChairAnimationType.PlayOpenWindow);
	}

	public void PlayChairAnimationCloseWindow()
	{
		ChangeChairAnimation(ChairAnimationType.PlayCloseWindow);
	}

	public void PlayChairAnimationStartUseCoffeeMaker()
	{
		ChangeChairAnimation(ChairAnimationType.StartUseCoffeeMaker);
	}

	public void PlayChairAnimationLoopUseCoffeeMaker()
	{
		ChangeChairAnimation(ChairAnimationType.LoopUseCoffeeMaker);
	}

	public void PlayChairAnimationEndUseCoffeeMaker()
	{
		ChangeChairAnimation(ChairAnimationType.EndUseCoffeeMaker);
	}

	public void PlayChairAnimationGreetingWave1()
	{
		ChangeChairAnimation(ChairAnimationType.Greeting_Wave_1);
	}

	public void PlayChairAnimationLeaveChairNormalGo()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Normal_Go);
	}

	public void PlayChairAnimationLeaveChairNormalComeback()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Normal_Comeback);
	}

	public void PlayChairAnimationLeaveChairCupcakeComeback()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Cupcake_Comeback);
	}

	public void PlayChairAnimationLeaveChairCupcakeShow()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Cupcake_Show_Loop);
	}

	public void PlayChairAnimationLeaveChairCupcakeEatStart1()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Cupcake_Eat_Start_1);
	}

	public void PlayChairAnimationLeaveChairCupcakeEatLoop1()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Cupcake_Eat_Loop_1);
	}

	public void PlayChairAnimationLeaveChairCupcakeEatStart2()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Cupcake_Eat_Start_2);
	}

	public void PlayChairAnimationLeaveChairCupcakeEatLoop2()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Cupcake_Eat_Loop_2);
	}

	public void PlayChairAnimationLeaveChairCupcakeEatEnd()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Cupcake_Eat_End);
	}

	public void PlayChairAnimationLeaveChairSofaGo()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Sofa_Go);
	}

	public void PlayChairAnimationLeaveChairSofaComeback()
	{
		ChangeChairAnimation(ChairAnimationType.LeaveChair_Sofa_Comeback);
	}

	private void ImmediateTidyingChairAnimation()
	{
		ChangeChairAnimation(ChairAnimationType.PlayEnd);
		_currentChairAnimator.Play("Chair_Idle", 0, 0f);
	}

	private void ImmediateTidyingPenAnimation()
	{
		ChangeChairAnimation(ChairAnimationType.PlayEnd);
		_penAnimator.Play("Pen_Idle", 0, 0f);
	}

	public bool IsCanOpenOrCloseWindow()
	{
		return _windowController.IsCanOpenOrCloseWindow();
	}

	public HeroineAI.ActionStateType GetNextOpenOrCloseState()
	{
		return _windowController.GetNextOpenOrCloseState();
	}

	public void PlayWindowAnimationOpenWindow()
	{
		_windowController.PlayWindowAnimationOpenWindow();
	}

	public void PlayWindowAnimationCloseWindow()
	{
		_windowController.PlayWindowAnimationCloseWindow();
	}

	private void ImmediateTidyingWindowAnimation()
	{
		_windowController.ImmediateTidyingAnimation();
	}

	public void PlayDropCoffeeFromCoffeeMaker()
	{
		if (!_coffeeDropParticle.isPlaying)
		{
			_coffeeDropParticle.Play();
		}
	}

	public void DeactivateDropCoffee()
	{
		_coffeeDropParticle.Stop();
		_coffeeDropParticle.Clear();
	}

	public void PlayBearsRestaurantBookAnim(string motionName)
	{
		_bearsRestaurantBook.Play(motionName);
	}

	public bool ParseDeskTypeForString(string type, out DeskType deskType)
	{
		if (Enum.TryParse<DeskType>(type, out deskType))
		{
			return true;
		}
		return false;
	}
}
