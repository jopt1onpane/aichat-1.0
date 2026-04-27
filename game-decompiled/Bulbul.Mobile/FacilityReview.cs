using System.Threading;
using Bulbul.Web;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityReview : MonoBehaviour
{
	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private LoadingScreen _loadingScreen;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private ScenarioReader scenarioReader;

	[Inject]
	private SceneFadeControllerProvider fadeControllerProvider;

	[Inject]
	private IPomodoroCompletedScheduler pomodoroCompletedScheduler;

	[SerializeField]
	private RectTransform _commonDialogParent;

	[SerializeField]
	[Header("機能群のアクティブチェック用")]
	private ObjectsActiveChecker _facilitiesWindowChecker;

	private ReviewState _reviewState;

	private bool _isRequestReview;

	public void Setup()
	{
		_reviewState = InMemoryData.GetOrSet(() => new ReviewState());
		_isRequestReview = false;
		_pomodoroService.OnPreAddExpAndPointFromCompletePomodoro.Subscribe(delegate(float exp)
		{
			if (!_reviewState.IsAlreadyReviewed && SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.None && SaveDataManager.Instance.LevelData.CurrentLevel >= 5)
			{
				float num = SaveDataManager.Instance.LevelData.CurrentExp + exp;
				if (!(num < SaveDataManager.Instance.LevelData.NextLevelNecessaryExp))
				{
					int num2 = SaveDataManager.Instance.LevelData.CurrentLevel + 1;
					_isRequestReview = num2 % 2 != 0;
					if (!_isRequestReview)
					{
						_isRequestReview = CheckMultipleLvUp(SaveDataManager.Instance.LevelData.CurrentLevel, num);
					}
				}
			}
		}).AddTo(this);
		_pomodoroService.OnCompletePomodoro.SubscribeAwait(async delegate(PomodoroService.PomodoroType _, CancellationToken ct)
		{
			if (_isRequestReview)
			{
				UniTask defer = UniTask.Defer((this, ct), ((FacilityReview @this, CancellationToken ct) x) => x.@this.ReviewAsync(x.ct));
				if (Condition())
				{
					await defer;
					_isRequestReview = false;
				}
				else
				{
					await pomodoroCompletedScheduler.Schedule(this, Condition, defer, base.destroyCancellationToken);
					_isRequestReview = false;
				}
			}
		}, AwaitOperation.Drop).AddTo(this);
		bool Condition()
		{
			if (_facilitiesWindowChecker.CheckActive())
			{
				ScreenOrientation orientation = Screen.orientation;
				if (orientation != ScreenOrientation.LandscapeLeft && orientation != ScreenOrientation.LandscapeRight)
				{
					goto IL_0039;
				}
			}
			if (!scenarioReader.IsPlayingLongStoryOrTutorial())
			{
				return fadeControllerProvider.Controller.IsComplete();
			}
			goto IL_0039;
			IL_0039:
			return false;
		}
	}

	private bool CheckMultipleLvUp(int curLv, float curExp)
	{
		int num = curLv + 1;
		int num2 = curLv - 1;
		int num3 = num - 1;
		float num4 = 0f;
		int num5 = _masterDataLoader.LevelUpInfoData.NextLevelNecessaryExpArray.Length;
		num4 = ((num2 >= num5) ? _masterDataLoader.LevelUpInfoData.NextLevelNecessaryExpBase : _masterDataLoader.LevelUpInfoData.NextLevelNecessaryExpArray[num2]);
		num4 = ((num3 >= num5) ? (num4 + _masterDataLoader.LevelUpInfoData.NextLevelNecessaryExpBase) : (num4 + _masterDataLoader.LevelUpInfoData.NextLevelNecessaryExpArray[num3]));
		return curExp >= num4;
	}

	private async UniTask ReviewAsync(CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		if (await CommonDialog.Create(delegate(CommonDialogOption op)
		{
			op.Parent = _commonDialogParent;
			op.TitleID = null;
			op.BodyID = "ui_review_request_body";
			op.UseCloseButton = false;
			op.EnableCloseOnClickButton = true;
			op.Buttons = new CommonButton[2]
			{
				new CommonButton("ui_review_request_review", CommonButtonStyle.Submit),
				new CommonButton("ui_review_request_later", CommonButtonStyle.Normal, SystemSeType.Cancel)
			};
		}).SubmitOrCloseWaitAsync(ct) == 0)
		{
			using (_loadingScreen.CreateLoadingScope())
			{
				await WebApi.PostAsync<SetIsReview, Nil>(new SetIsReview(SaveDataManager.Instance.AccountData.DeviceID), ct);
				_reviewState.IsAlreadyReviewed = true;
				InMemoryData.SetData(_reviewState);
				await ReviewLogic.RequestReviewAsync(this.GetCancellationTokenOnDestroy());
			}
		}
	}
}
