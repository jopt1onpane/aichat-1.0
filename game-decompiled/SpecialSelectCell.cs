using Bulbul.MasterData;
using R3;
using UnityEngine;

public class SpecialSelectCell : MonoBehaviour
{
	[SerializeField]
	private SpecialSelectCellView _view;

	protected int _episodeCountMax;

	protected ReactiveProperty<int> _readEpisodeCount = new ReactiveProperty<int>();

	private SpecialService.CollaborationType _specialType;

	public SpecialService.CollaborationType SpecialType => _specialType;

	public Observable<Unit> OnSubmit => _view.OnSubmit;

	public virtual void Setup(SpecialService.CollaborationType specialType, Observable<ScenarioType> onEndStory)
	{
		_specialType = specialType;
		_view.Setup(_episodeCountMax, _readEpisodeCount);
	}

	public void Activate()
	{
		_view.Activate();
	}

	public void Deactivate()
	{
		_view.Deactivate();
	}

	public void GrayOut()
	{
		_view.GrayOut();
	}

	public void OnAddValue()
	{
		_view.OnAddValue();
	}
}
