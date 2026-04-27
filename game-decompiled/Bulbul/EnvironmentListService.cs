using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class EnvironmentListService : MonoBehaviour, IEnvironmentUIService
{
	[Inject]
	private UnlockItemService _unlockItemService;

	[SerializeField]
	private AdjustGridLayout[] _environmentGridLayout;

	[SerializeField]
	private AdjustGridLayout _viewAndSoundGridLayout;

	[SerializeField]
	private AdjustGridLayout _viewGridLayout;

	[SerializeField]
	private AdjustGridLayout _soundGridLayout;

	private DateTime _lastChangeWindowDateTime;

	private List<EnvironmentController> _viewAndSoundControllerList = new List<EnvironmentController>();

	private List<EnvironmentController> _viewControllerList = new List<EnvironmentController>();

	private List<EnvironmentController> _soundControllerList = new List<EnvironmentController>();

	private List<EnvironmentType> _viewAndSoundOrderList = new List<EnvironmentType>();

	private List<EnvironmentType> _viewOrderList = new List<EnvironmentType>();

	private List<EnvironmentType> _soundOrderList = new List<EnvironmentType>();

	private Dictionary<AdjustGridLayout, Queue<Action>> _reorderQueues = new Dictionary<AdjustGridLayout, Queue<Action>>();

	private Dictionary<AdjustGridLayout, bool> _isProcessing = new Dictionary<AdjustGridLayout, bool>();

	public DateTime LastChangeWindowDateTime => _lastChangeWindowDateTime;

	public void Setup()
	{
		AdjustGridLayout[] environmentGridLayout = _environmentGridLayout;
		for (int i = 0; i < environmentGridLayout.Length; i++)
		{
			environmentGridLayout[i].Setup();
		}
		_lastChangeWindowDateTime = DateTime.Now;
		EnvironmentSetup(_viewAndSoundGridLayout, ref _viewAndSoundControllerList, ref _viewAndSoundOrderList);
		EnvironmentSetup(_viewGridLayout, ref _viewControllerList, ref _viewOrderList);
		EnvironmentSetup(_soundGridLayout, ref _soundControllerList, ref _soundOrderList);
	}

	private void EnqueueReorder(AdjustGridLayout gridLayout, Action reorderAction)
	{
		if (!_reorderQueues.ContainsKey(gridLayout))
		{
			_reorderQueues[gridLayout] = new Queue<Action>();
			_isProcessing[gridLayout] = false;
		}
		_reorderQueues[gridLayout].Enqueue(reorderAction);
		if (!_isProcessing[gridLayout])
		{
			ProcessReorderQueue(gridLayout, this.GetCancellationTokenOnDestroy()).Forget();
		}
	}

	private async UniTask ProcessReorderQueue(AdjustGridLayout gridLayout, CancellationToken cancellationToken)
	{
		_isProcessing[gridLayout] = true;
		try
		{
			while (_reorderQueues[gridLayout].Count > 0 && !cancellationToken.IsCancellationRequested)
			{
				_reorderQueues[gridLayout].Dequeue()?.Invoke();
				await UniTask.Yield(cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			_isProcessing[gridLayout] = false;
		}
	}

	private void EnvironmentSetup(AdjustGridLayout gridLayout, ref List<EnvironmentController> controllerList, ref List<EnvironmentType> orderList)
	{
		EnvironmentController[] componentsInChildren = gridLayout.GetComponentsInChildren<EnvironmentController>();
		foreach (EnvironmentController environmentController in componentsInChildren)
		{
			controllerList.Add(environmentController);
			orderList.Add(environmentController.EnvironmentType);
		}
		List<EnvironmentType> tempOrderList = orderList;
		foreach (EnvironmentController controller in controllerList)
		{
			if (controller.IsLock())
			{
				controller.transform.SetAsLastSibling();
			}
			_unlockItemService.Environment.GetLockState(controller.EnvironmentType).IsLocked.Skip(1).Subscribe(delegate(bool isLock)
			{
				if (!isLock)
				{
					EnqueueReorder(gridLayout, delegate
					{
						bool flag = false;
						int num = -1;
						int num2 = -1;
						EnvironmentController[] componentsInChildren2 = gridLayout.GetComponentsInChildren<EnvironmentController>();
						foreach (EnvironmentController environmentController3 in componentsInChildren2)
						{
							num2++;
							if (controller.EnvironmentType == environmentController3.EnvironmentType || environmentController3.IsLock())
							{
								break;
							}
							foreach (EnvironmentType item in tempOrderList)
							{
								if (controller.EnvironmentType == item)
								{
									flag = true;
									break;
								}
								if (item == environmentController3.EnvironmentType)
								{
									if (num < num2)
									{
										num = num2;
									}
									break;
								}
							}
							if (flag)
							{
								break;
							}
						}
						int num3 = 0;
						num3 = (flag ? num2 : ((num != -1) ? (num + 1) : 0));
						controller.transform.SetSiblingIndex(num3);
					});
				}
			}).AddTo(this);
		}
		componentsInChildren = _viewAndSoundGridLayout.GetComponentsInChildren<EnvironmentController>();
		foreach (EnvironmentController environmentController2 in componentsInChildren)
		{
			controllerList.Add(environmentController2);
			orderList.Add(environmentController2.EnvironmentType);
		}
	}

	public void AdjustRectSize()
	{
		AdjustGridLayout[] environmentGridLayout = _environmentGridLayout;
		for (int i = 0; i < environmentGridLayout.Length; i++)
		{
			environmentGridLayout[i].AdjustRectSize();
		}
	}

	public void SetLastChangeWindowDateTime()
	{
		_lastChangeWindowDateTime = DateTime.Now;
	}
}
