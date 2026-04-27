using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NestopiSystem;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class StorySelectUI : MonoBehaviour, IStorySelectUI
{
	[Inject]
	private ScenarioGroupMasterWrapper scenarioGroupMaster;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private ChangeOrderService _changeOrderService;

	[Inject]
	private FacilityLockEventService _facilityLockEventService;

	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[Header("レイキャストの対象になるようにImageを同じオブジェクトにアタッチする必要がある")]
	[SerializeField]
	[Header("ストーリー選択リストオブジェクト")]
	private GameObject _uiParent;

	[SerializeField]
	[Header("ストーリー選択リストオブジェクトの親")]
	private RectTransform cellParent;

	[SerializeField]
	[Header("ストーリー選択リストオブジェクト")]
	private StoryCellUI cellPrefab;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	[Header("ストーリーロック用")]
	private LockUI _lockUI;

	[SerializeField]
	[Header("ScrollRect")]
	private ScrollRect _scrollRect;

	[SerializeField]
	[Header("タグ切り替えボタン: メインストーリー")]
	private Button _tagChangeToMainButton;

	[SerializeField]
	[Header("タグ切り替えボタン: インタラクトUI")]
	private InteractableUI _tagChangeToMainInteractableUI;

	[SerializeField]
	[Header("タグ切り替えボタン: スペシャル")]
	private Button _tagChangeToSpecialButton;

	[SerializeField]
	[Header("タグ切り替えボタン: インタラクトUI")]
	private InteractableUI _tagChangeToSpecialInteractableUI;

	[SerializeField]
	private RectTransform categoryTitlePrefab;

	[SerializeField]
	[Header("閉じるボタン")]
	private Button _closeButton;

	private readonly ReactiveProperty<bool> storyGate = new ReactiveProperty<bool>(value: true);

	private bool playableScenarioDirty;

	private DisposableBag storyListDisposable;

	private FacilityStory facilityStory;

	private InteractableUI _currentUseUI;

	private InteractableUI _currentMouseOverUI;

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private float _fromPosY;

	private float _toPosY;

	private bool _isActive;

	public Observable<Unit> OnClickTagChangeToMainButton => _tagChangeToMainButton.OnClickAsObservable();

	public Observable<Unit> OnClickTagChangeToSpecialButton => _tagChangeToSpecialButton.OnClickAsObservable();

	public Observable<Unit> OnClickCloseButton => _closeButton.OnClickAsObservable();

	public void Setup(FacilityStory facilityStory)
	{
		this.facilityStory = facilityStory;
		CreateList();
		_tagChangeToMainInteractableUI.Setup();
		_tagChangeToSpecialInteractableUI.Setup();
		facilityStory.PlayableScenario.ObserveCountChanged().Subscribe(this, delegate(int _, StorySelectUI @this)
		{
			@this.playableScenarioDirty = true;
		}).AddTo(this);
		_rectTransform = base.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
		_lockUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnLock, delegate
		{
			Deactivate();
			_lockUI.Activate();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnUnlock, delegate
		{
			_lockUI.Deactivate();
		}).AddTo(this);
		facilityStory.CurrentStoryTag.Subscribe(delegate(FacilityStory.StoryTag currentStoryTag)
		{
			switch (currentStoryTag)
			{
			case FacilityStory.StoryTag.Main:
				_tagChangeToMainInteractableUI.ActivateUseUI();
				_tagChangeToSpecialInteractableUI.DeactivateAllUI();
				break;
			case FacilityStory.StoryTag.Special:
				_tagChangeToSpecialInteractableUI.ActivateUseUI();
				_tagChangeToMainInteractableUI.DeactivateAllUI();
				break;
			}
		}).AddTo(this);
	}

	private void CreateList()
	{
		while (cellParent.childCount > 0)
		{
			Object.DestroyImmediate(cellParent.GetChild(0).gameObject);
		}
		storyListDisposable.DisposeAndRecreate(facilityStory.PlayableScenario.Count);
		Dictionary<ScenarioType, int> scenarioOrder = new Dictionary<ScenarioType, int>
		{
			{
				ScenarioType.Tutorial,
				0
			},
			{
				ScenarioType.MainScenario,
				1
			},
			{
				ScenarioType.Special_AlterEgo,
				2
			},
			{
				ScenarioType.Special_BearsRestaurant,
				3
			},
			{
				ScenarioType.Special_Valentine2026,
				4
			},
			{
				ScenarioType.Special_LunaNewYear2026,
				4
			},
			{
				ScenarioType.Special_NearSpring2026,
				5
			}
		};
		foreach (IGrouping<ScenarioType, ScenarioGroupData> item in from x in facilityStory.PlayableScenario
			group x by x.Scenario into g
			orderby (!scenarioOrder.ContainsKey(g.Key)) ? int.MaxValue : scenarioOrder[g.Key]
			select g)
		{
			switch (item.Key)
			{
			case ScenarioType.Special_AlterEgo:
				Object.Instantiate(categoryTitlePrefab, cellParent, worldPositionStays: false).GetComponentInChildren<TextLocalizationBehaviour>().Set("special_alterego_title");
				break;
			case ScenarioType.Special_BearsRestaurant:
				Object.Instantiate(categoryTitlePrefab, cellParent, worldPositionStays: false).GetComponentInChildren<TextLocalizationBehaviour>().Set("special_bearsrestaurant_title");
				break;
			case ScenarioType.Special_Valentine2026:
				Object.Instantiate(categoryTitlePrefab, cellParent, worldPositionStays: false).GetComponentInChildren<TextLocalizationBehaviour>().Set("special_valentine2026_title");
				break;
			case ScenarioType.Special_LunaNewYear2026:
				Object.Instantiate(categoryTitlePrefab, cellParent, worldPositionStays: false).GetComponentInChildren<TextLocalizationBehaviour>().Set("special_lunaNewYear2026_title");
				break;
			case ScenarioType.Special_NearSpring2026:
				Object.Instantiate(categoryTitlePrefab, cellParent, worldPositionStays: false).GetComponentInChildren<TextLocalizationBehaviour>().Set("special_season_base_title");
				break;
			}
			foreach (ScenarioGroupData data in item.OrderBy((ScenarioGroupData x) => x.EpisodeNumber))
			{
				StoryCellUI storyCellUI = Object.Instantiate(cellPrefab, cellParent, worldPositionStays: false);
				storyCellUI.Setup(data);
				storyCellUI.OnSubmit.SubscribeAwait(async delegate(ScenarioGroupData _, CancellationToken ct)
				{
					facilityStory.StartStory(data.Scenario, data.EpisodeNumber, isTalkLog: true);
					await UniTask.WaitUntil(() => facilityStory.IsStoryPlayEnd(), PlayerLoopTiming.Update, ct);
				}, storyGate).AddTo(ref storyListDisposable);
			}
		}
	}

	private void LateUpdate()
	{
		if (playableScenarioDirty)
		{
			playableScenarioDirty = false;
			CreateList();
		}
	}

	public bool IsActive()
	{
		return _isActive;
	}

	public void Activate()
	{
		_changeOrderService.BringToFront(ChangeOrderService.OrderItemType.Story);
		_isActive = true;
		_facilityOpenButton.ActivateUseUI();
		_uiParent.SetActive(value: true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.gameObject.GetComponent<RectTransform>());
		_scrollRect.verticalNormalizedPosition = 1f;
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
	}

	public void Deactivate()
	{
		_isActive = false;
		_facilityOpenButton.DeactivateUseUI();
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			_uiParent.SetActive(value: false);
		});
	}
}
