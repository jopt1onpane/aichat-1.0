using Bulbul.MasterData;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class StoryCellUIForMobile : StoryCellUI
{
	[SerializeField]
	private GameObject lockUIRoot;

	[SerializeField]
	private Button lockButton;

	public Observable<Unit> OnClickLockButton => lockButton.OnClickAsObservable();

	public override void Setup(ScenarioGroupData master)
	{
		SetupLock(master);
		base.Setup(master);
	}

	private void SetupLock(ScenarioGroupData master)
	{
	}
}
