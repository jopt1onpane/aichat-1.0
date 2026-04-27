using System;
using VContainer;

namespace Bulbul;

public class OneTimePointGrantService
{
	[Inject]
	private PlayerPointService _playerPointService;

	public void CheckAndGrantFirstPoint()
	{
		PlayerDataV3 playerData = SaveDataManager.Instance.PlayerData;
		if (!playerData.HasReceivedFirstPoint)
		{
			DateTime dateTime = new DateTime(2025, 12, 25);
			if (DateTime.Now >= dateTime)
			{
				_playerPointService.AddPoint(10000);
				playerData.HasReceivedFirstPoint = true;
				SaveDataManager.Instance.SavePlayerData();
			}
		}
	}
}
