using System.Diagnostics;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class PlayerLevelUIService : MonoBehaviour, IPlayerLevelUIService
{
	private enum AnimationTrigger
	{
		Start_LevelUp
	}

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private PlayerLevelService _playerLevelService;

	[SerializeField]
	[Header("経験値取得量UI")]
	private AddExpValueUIPool _addExpValueUIPool;

	[SerializeField]
	[Header("経験値取得量UIの表示秒数")]
	private float _addExpValueUIStaySeconds;

	[SerializeField]
	[Header("メーター画像\u3000経験値量によって上から表示するUI")]
	private AddExpMeterUI _addExpMeterUI;

	[SerializeField]
	[Header("経験値上昇時に回転させるアニメーション")]
	private AddExpOutsideRotateAnim _addExpOutsideRotateAnim;

	[SerializeField]
	[Header("表示レベルタイトルテキスト")]
	private TextMeshProUGUI _levelTitleText;

	[SerializeField]
	[Header("表示レベルテキスト")]
	private TextMeshProUGUI _showLevelText;

	[SerializeField]
	[Header("レベルアップAnimator")]
	private Animator _levelUpAnimator;

	[SerializeField]
	[Header("レベルアップテキストフェード秒数")]
	private float _levelUpTextFadeSeconds;

	[SerializeField]
	[Header("レベルアップテキストフェードアウトのDelay秒数")]
	private float _levelUpTextFadeOutDelaySeconds;

	[SerializeField]
	[Header("ベース画像：通常")]
	private GameObject _levelNormalBaseObject;

	[SerializeField]
	[Header("ベース画像：オルターエゴコラボ")]
	private GameObject _levelAlterEgoBaseObject;

	[SerializeField]
	[Header("ベース画像：くまのレストランコラボ")]
	private GameObject _levelBearsRestaurantBaseObject;

	[SerializeField]
	[Header("ベース画像：バレンタイン2026")]
	private GameObject _levelValentineBaseObject;

	[SerializeField]
	[Header("ベース画像：旧正月2026")]
	private GameObject _levelLunaNewYear2026BaseObject;

	[SerializeField]
	[Header("ベース画像：NearSpring2026")]
	private GameObject _levelNearSpring2026BaseObject;

	[SerializeField]
	[Header("フォーカスUI")]
	private TutorialFocusUI focusUI;

	private Subject<LevelData> _onEndShowExpValue = new Subject<LevelData>();

	private Subject<LevelData> _onUpdateUILevelData = new Subject<LevelData>();

	private Subject<float> _onAddNotAddedYetShowExp = new Subject<float>();

	private Subject<LevelData> _onEndAddExp = new Subject<LevelData>();

	private Subject<LevelData> _onReadyLevelUP = new Subject<LevelData>();

	private Subject<Unit> _onStartLevelUP = new Subject<Unit>();

	private Subject<LevelData> _onEndLevelUP = new Subject<LevelData>();

	private LevelData _showLevelData;

	private float _notAddedYetShowExp;

	private float _gaugeUpValue;

	private Stopwatch _expValueAnimStopWatch = new Stopwatch();

	public string CurrentShowLevel => _showLevelText.text;

	public Observable<LevelData> OnEndShowExpValue => _onEndShowExpValue;

	public Observable<LevelData> OnUpdateUILevelData => _onUpdateUILevelData;

	public Observable<float> OnAddNotAddedYetShowExp => _onAddNotAddedYetShowExp;

	public Observable<LevelData> OnEndAddExp => _onEndAddExp;

	public Observable<LevelData> OnReadyLevelUP => _onReadyLevelUP;

	public Observable<Unit> OnStartLevelUP => _onStartLevelUP;

	public Observable<LevelData> OnEndLevelUP => _onEndLevelUP;

	public float NotAddedYetShowExp => _notAddedYetShowExp;

	public void Setup()
	{
		LevelData currentLevelData = LevelData.GetCurrentLevelData();
		_showLevelData = ((currentLevelData != null) ? new LevelData(currentLevelData) : new LevelData(SaveDataManager.Instance.LevelData));
		_showLevelText.text = _showLevelData.CurrentLevel.ToString();
		_addExpOutsideRotateAnim.Setup();
		_addExpMeterUI.Setup();
		_addExpValueUIPool.Setup();
		ObservableSubscribeExtensions.Subscribe(SaveDataManager.Instance.CollaborationSaveData.CurrentType, delegate
		{
			UpdateCollaborationUI();
		}).AddTo(this);
		void UpdateCollaborationUI()
		{
			_levelNormalBaseObject.SetActive(value: false);
			_levelAlterEgoBaseObject.SetActive(value: false);
			_levelBearsRestaurantBaseObject.SetActive(value: false);
			_levelValentineBaseObject.SetActive(value: false);
			_levelLunaNewYear2026BaseObject.SetActive(value: false);
			_levelNearSpring2026BaseObject.SetActive(value: false);
			_showLevelText.gameObject.SetActive(value: false);
			_levelTitleText.gameObject.SetActive(value: false);
			switch (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value)
			{
			case SpecialService.CollaborationType.None:
				_levelNormalBaseObject.SetActive(value: true);
				_showLevelText.gameObject.SetActive(value: true);
				_levelTitleText.gameObject.SetActive(value: true);
				break;
			case SpecialService.CollaborationType.AlterEgo:
				_levelAlterEgoBaseObject.SetActive(value: true);
				break;
			case SpecialService.CollaborationType.BearsRestaurant:
				_levelBearsRestaurantBaseObject.SetActive(value: true);
				break;
			case SpecialService.CollaborationType.Valentine2026:
				_levelValentineBaseObject.SetActive(value: true);
				break;
			case SpecialService.CollaborationType.LunaNewYear2026:
				_levelLunaNewYear2026BaseObject.SetActive(value: true);
				break;
			case SpecialService.CollaborationType.NearSpring2026:
				_levelNearSpring2026BaseObject.SetActive(value: true);
				break;
			}
			LevelData currentLevelData2 = LevelData.GetCurrentLevelData();
			if (currentLevelData2 != null)
			{
				_showLevelData = new LevelData(currentLevelData2);
				_showLevelText.text = _showLevelData.CurrentLevel.ToString();
			}
			_addExpMeterUI.SyncWithSaveData();
		}
	}

	public void UpdateUI()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (_expValueAnimStopWatch.Elapsed.TotalSeconds >= (double)_addExpValueUIStaySeconds)
			{
				_expValueAnimStopWatch.Reset();
				_onEndShowExpValue.OnNext(_showLevelData);
			}
			UpdateExpGauge();
		}
	}

	private void UpdateExpGauge()
	{
		if (_notAddedYetShowExp <= 0f)
		{
			_playerLevelService.OnFinishLevelUpDirection();
			return;
		}
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.AlterEgo && SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LevelData.CurrentLevel >= 2 && _showLevelData.CurrentExp >= _showLevelData.NextLevelNecessaryExp)
		{
			_notAddedYetShowExp = 0f;
			_onEndAddExp.OnNext(_showLevelData);
			return;
		}
		float num = _gaugeUpValue * Time.deltaTime;
		if (_notAddedYetShowExp - num < 0f)
		{
			num = _notAddedYetShowExp;
		}
		if (_showLevelData.NextLevelNecessaryExp < _showLevelData.CurrentExp + num)
		{
			num = _showLevelData.NextLevelNecessaryExp - _showLevelData.CurrentExp;
		}
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.AlterEgo && SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LevelData.CurrentLevel >= 2)
		{
			float num2 = _showLevelData.NextLevelNecessaryExp - _showLevelData.CurrentExp;
			if (num > num2)
			{
				num = num2;
			}
		}
		_showLevelData.AddExp(num);
		_onUpdateUILevelData.OnNext(_showLevelData);
		_notAddedYetShowExp -= num;
		if (_notAddedYetShowExp <= 0f)
		{
			_onEndAddExp.OnNext(_showLevelData);
			_playerLevelService.OnFinishLevelUpDirection();
		}
	}

	public bool IsAccumulatedLevelUpExp()
	{
		LevelData currentLevelData = LevelData.GetCurrentLevelData();
		if (currentLevelData != null && Mathf.Ceil(_showLevelData.CurrentExp) >= _showLevelData.NextLevelNecessaryExp && _showLevelData.CurrentLevel < currentLevelData.CurrentLevel)
		{
			_onEndAddExp.OnNext(_showLevelData);
			return true;
		}
		return false;
	}

	public void StartLevelUpDirection()
	{
		_onStartLevelUP.OnNext(Unit.Default);
		if (!_directionService.GamePlayingDefect.IsConnectionLost())
		{
			_systemSeService.PlayLevelUp();
		}
		_levelUpAnimator.SetTrigger(AnimationTrigger.Start_LevelUp.ToString());
		_showLevelData.LevelUp(_masterDataLoader);
		_showLevelText.DOFade(0f, _levelUpTextFadeSeconds).OnComplete(delegate
		{
			_showLevelText.text = _showLevelData.CurrentLevel.ToString();
			_showLevelText.DOFade(1f, _levelUpTextFadeSeconds).SetDelay(_levelUpTextFadeOutDelaySeconds);
		});
	}

	public void EndLevelUpDirection()
	{
		_onEndLevelUP.OnNext(_showLevelData);
	}

	public bool IsEndLevelUpDirection()
	{
		return !MyAnimatorUtil.IsCurrentAnimation(_levelUpAnimator, AnimationTrigger.Start_LevelUp.ToString());
	}

	public void OnAddExp(float exp)
	{
		_addExpValueUIPool.AddExpValueUI(exp, OnEndAnimation);
		_expValueAnimStopWatch.Restart();
		void OnEndAnimation()
		{
			_notAddedYetShowExp += exp;
			_onAddNotAddedYetShowExp.OnNext(exp);
			_gaugeUpValue = _notAddedYetShowExp * _masterDataLoader.LevelUpInfoData.GaugeUpSpeed;
		}
	}

	public void FocusUIActivate()
	{
		if ((bool)focusUI)
		{
			focusUI.Activate();
		}
	}

	public void FocusUIDeactivate()
	{
		if ((bool)focusUI)
		{
			focusUI.Deactivate();
		}
	}

	public void SyncWithSaveData()
	{
		LevelData currentLevelData = LevelData.GetCurrentLevelData();
		_showLevelData = ((currentLevelData != null) ? new LevelData(currentLevelData) : new LevelData(SaveDataManager.Instance.LevelData));
		_showLevelText.text = _showLevelData.CurrentLevel.ToString();
		_notAddedYetShowExp = 0f;
		_gaugeUpValue = 0f;
		_expValueAnimStopWatch.Reset();
		_addExpMeterUI.SyncWithSaveData();
	}
}
