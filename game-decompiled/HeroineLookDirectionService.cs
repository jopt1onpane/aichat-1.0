using System;
using Bulbul;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

public class HeroineLookDirectionService : MonoBehaviour
{
	[Inject]
	private ScenarioReader _scenarioReader;

	private const string LookScaleKey = "LookScale";

	[Header("視線制御")]
	[SerializeField]
	private SkinnedMeshRenderer _faceSkin;

	[SerializeField]
	private Transform _headTransform;

	[SerializeField]
	private Transform _eyeTransform;

	[SerializeField]
	private Transform _bodyTransform;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("視線制御の際に頭・首を動かす比重")]
	private float _lookAtHeadWeight = 0.8f;

	[SerializeField]
	[Tooltip("LookAtWeightが0から1に移動する間の目の制御比重の変化")]
	private Ease _eyeLookAtWeightEase;

	[SerializeField]
	[Tooltip("この値分、対象より上の位置に顔を向ける")]
	private float _lookAtTargetYOffset;

	[SerializeField]
	[Tooltip("顔を向ける対象")]
	private Transform _lookAtTarget;

	[SerializeField]
	[Tooltip("顔を向ける量")]
	[Range(0f, 1f)]
	private float _lookAtWeight;

	private Animator _animator;

	private const int LookAtIKAnimationLayer = 0;

	private float _toLookAtWeight;

	private float _toLookAtWeightAdjust;

	private float _toLookSeconds;

	private Tween _lookTweenAnimator;

	private Tween _lookTweenManual;

	private Ease _lookEase;

	private bool _isUsingFromLook;

	private HeroineEyeLookAt _heroineEyeLookAt;

	private HeroineService _heroineService;

	public void Setup(Animator animator, HeroineService heroineService)
	{
		_animator = animator;
		_heroineEyeLookAt = new HeroineEyeLookAt(_faceSkin, _headTransform);
		_heroineService = heroineService;
	}

	public bool IsLookInitDirection()
	{
		if (_animator.GetFloat("LookScale") == 0f && _lookAtWeight == 0f)
		{
			return true;
		}
		return false;
	}

	public void ChangeLookScale(float lookScale, float lookSpeedSeconds, Ease lookEaseType = Ease.Unset)
	{
		float startValue = _animator.GetFloat("LookScale");
		_lookTweenAnimator?.Kill();
		_lookTweenAnimator = DOTween.To(() => startValue, delegate(float x)
		{
			startValue = x;
			_animator.SetFloat("LookScale", x);
		}, lookScale, lookSpeedSeconds).SetEase(lookEaseType);
	}

	public void PlayToLook()
	{
		_isUsingFromLook = false;
		float endValue = Mathf.Clamp(_toLookAtWeight, 0f, _toLookAtWeightAdjust);
		_lookTweenManual?.Kill();
		_lookTweenManual = DOTween.To(() => _lookAtWeight, delegate(float x)
		{
			_lookAtWeight = x;
		}, endValue, _toLookSeconds).SetEase(_lookEase);
	}

	public async UniTask PlayFromLook(float fromLookSpeed = 1f, Ease ease = Ease.Linear)
	{
		if (_isUsingFromLook)
		{
			return;
		}
		_isUsingFromLook = true;
		await UniTask.Delay(TimeSpan.FromSeconds(0.5));
		if (_isUsingFromLook)
		{
			_lookTweenManual?.Kill();
			_lookTweenManual = DOTween.To(() => _lookAtWeight, delegate(float x)
			{
				_lookAtWeight = x;
			}, 0f, fromLookSpeed).SetEase(ease).OnComplete(delegate
			{
				_isUsingFromLook = false;
			});
			ChangeLookScale(0f, fromLookSpeed, ease);
		}
	}

	public void InitLookImmediate()
	{
		_lookTweenManual?.Kill();
		_lookAtWeight = 0f;
		_isUsingFromLook = false;
		_lookTweenAnimator?.Kill();
		_animator.SetFloat("LookScale", 0f);
	}

	public void ManualLateUpdate()
	{
		if (_lookAtWeight > 0f && (bool)_lookAtTarget)
		{
			Vector3 vector = (_lookAtTarget.position - _eyeTransform.position).normalized;
			if (vector == Vector3.zero)
			{
				vector = _headTransform.forward;
			}
			float lookAtWeight = _lookAtWeight;
			lookAtWeight = DOVirtual.EasedValue(0f, 1f, lookAtWeight, _eyeLookAtWeightEase);
			vector = Vector3.Slerp(_eyeTransform.forward, vector, lookAtWeight);
			_heroineEyeLookAt.LookAtDirection(vector);
		}
	}

	public void ApplyValue(int layerIndex)
	{
		if (!(_animator == null) && !(_lookAtTarget == null) && layerIndex == 0)
		{
			_animator.SetLookAtPosition(_lookAtTarget.position + Vector3.up * _lookAtTargetYOffset);
			_animator.SetLookAtWeight(_lookAtWeight * _lookAtHeadWeight);
		}
	}

	public void ChangeLookScaleByManual(float lookScale, float lookSpeedSeconds, Ease lookEaseType)
	{
		if (lookScale > 0f)
		{
			AdjustLookParam();
			_toLookAtWeight = lookScale;
			_toLookSeconds = lookSpeedSeconds;
			PlayToLook();
		}
		else
		{
			PlayFromLook(lookSpeedSeconds).Forget();
		}
	}

	public void ChangeLookScaleByManualForStory(float lookScale, float lookSpeedSeconds, Ease lookEaseType)
	{
		switch (_scenarioReader.PlayingScenarioType)
		{
		case ScenarioType.HeroineClickNormal:
		case ScenarioType.HeroineClickWork:
		case ScenarioType.HeroineClickBreak:
		case ScenarioType.HeroineSelfShortTalkNormal:
		case ScenarioType.HeroineSelfShortTalkBreak:
		case ScenarioType.HeroineClickWork_Morning:
		case ScenarioType.HeroineClickWork_Noon:
		case ScenarioType.HeroineClickWork_Evening:
		case ScenarioType.HeroineClickWork_Night:
		case ScenarioType.HeroineClickBreak_Morning:
		case ScenarioType.HeroineClickBreak_Noon:
		case ScenarioType.HeroineClickBreak_Evening:
		case ScenarioType.HeroineClickBreak_Night:
		case ScenarioType.HeroineSelfShortTalkBreak_Morning:
		case ScenarioType.HeroineSelfShortTalkBreak_Noon:
		case ScenarioType.HeroineSelfShortTalkBreak_Evening:
		case ScenarioType.HeroineSelfShortTalkBreak_Night:
			AdjustLookParam();
			_toLookAtWeight = lookScale;
			_lookEase = lookEaseType;
			_toLookSeconds = lookSpeedSeconds;
			PlayToLook();
			break;
		default:
			ChangeLookScale(lookScale, lookSpeedSeconds, lookEaseType);
			break;
		}
	}

	private void AdjustLookParam()
	{
		if (_lookTweenManual != null && _lookTweenManual.IsActive() && !_lookTweenManual.IsComplete())
		{
			return;
		}
		HeroineAI.ActionStateType actionStateType = _heroineService.GetCurrentAIState();
		if (actionStateType == HeroineAI.ActionStateType.ClickHeroine)
		{
			switch (_scenarioReader.PlayingScenarioType)
			{
			case ScenarioType.HeroineClickNormal:
			case ScenarioType.HeroineClickWork:
			case ScenarioType.HeroineClickBreak:
			case ScenarioType.HeroineSelfShortTalkNormal:
			case ScenarioType.HeroineSelfShortTalkBreak:
			case ScenarioType.HeroineClickWork_Morning:
			case ScenarioType.HeroineClickWork_Noon:
			case ScenarioType.HeroineClickWork_Evening:
			case ScenarioType.HeroineClickWork_Night:
			case ScenarioType.HeroineClickBreak_Morning:
			case ScenarioType.HeroineClickBreak_Noon:
			case ScenarioType.HeroineClickBreak_Evening:
			case ScenarioType.HeroineClickBreak_Night:
			case ScenarioType.HeroineSelfShortTalkBreak_Morning:
			case ScenarioType.HeroineSelfShortTalkBreak_Noon:
			case ScenarioType.HeroineSelfShortTalkBreak_Evening:
			case ScenarioType.HeroineSelfShortTalkBreak_Night:
				actionStateType = _heroineService.GetBeforeAIState();
				break;
			}
		}
		switch (actionStateType)
		{
		case HeroineAI.ActionStateType.WorkPC:
			_lookAtHeadWeight = 0.333f;
			_lookAtTargetYOffset = 0f;
			_toLookAtWeightAdjust = 1f;
			break;
		case HeroineAI.ActionStateType.WorkBook:
			_lookAtHeadWeight = 0.426f;
			_lookAtTargetYOffset = 0.13f;
			_toLookAtWeightAdjust = 1f;
			break;
		case HeroineAI.ActionStateType.WorkReport:
			_lookAtHeadWeight = 0.397f;
			_lookAtTargetYOffset = 0.09f;
			_toLookAtWeightAdjust = 0.823f;
			break;
		case HeroineAI.ActionStateType.BreakMovie:
			_lookAtHeadWeight = 0.489f;
			_lookAtTargetYOffset = 1.26f;
			_toLookAtWeightAdjust = 0.688f;
			break;
		case HeroineAI.ActionStateType.BreakReadBook:
			_lookAtHeadWeight = 0.494f;
			_lookAtTargetYOffset = 1.74f;
			_toLookAtWeightAdjust = 0.8f;
			break;
		case HeroineAI.ActionStateType.BreakListenMusic:
			_lookAtHeadWeight = 0.508f;
			_lookAtTargetYOffset = 0.09f;
			_toLookAtWeightAdjust = 0.887f;
			break;
		case HeroineAI.ActionStateType.BreakTeaTime:
			_lookAtHeadWeight = 0.406f;
			_lookAtTargetYOffset = 0.01f;
			_toLookAtWeightAdjust = 0.976f;
			break;
		case HeroineAI.ActionStateType.BreakSleep:
			_lookAtHeadWeight = 0f;
			_lookAtTargetYOffset = 0f;
			_toLookAtWeightAdjust = 0f;
			break;
		case HeroineAI.ActionStateType.WantTalk:
			_lookAtHeadWeight = 0.38f;
			_lookAtTargetYOffset = 0f;
			_toLookAtWeightAdjust = 1f;
			break;
		default:
			_lookAtHeadWeight = 0.38f;
			_lookAtTargetYOffset = 0f;
			_toLookAtWeightAdjust = 1f;
			break;
		}
	}
}
