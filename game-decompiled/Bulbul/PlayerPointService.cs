using System;
using R3;
using VContainer;

namespace Bulbul;

public class PlayerPointService
{
	[Inject]
	private MasterDataLoader _masterDataLoader;

	private readonly Subject<int> _onPointChange = new Subject<int>();

	private SaveDataManager SaveData => SaveDataManager.Instance;

	public Observable<int> OnPointChange => _onPointChange;

	public int Point => SaveData.PointPurchaseData.Point;

	public void Setup()
	{
		SaveData.PointPurchaseData.Point = Math.Max(Point, 0);
	}

	public void AddPoint(int point)
	{
		SaveData.PointPurchaseData.Point += point;
		SaveData.SavePointPurchaseData();
		_onPointChange.OnNext(point);
	}

	public bool ConsumePoint(int point, bool save)
	{
		if (SaveData.PointPurchaseData.Point < point)
		{
			return false;
		}
		SaveData.PointPurchaseData.Point -= point;
		if (save)
		{
			SaveData.SavePointPurchaseData();
		}
		_onPointChange.OnNext(-point);
		return true;
	}

	public void SavePoint()
	{
		SaveData.SavePointPurchaseData();
	}
}
