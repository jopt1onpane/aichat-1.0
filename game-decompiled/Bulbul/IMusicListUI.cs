using ObservableCollections;

namespace Bulbul;

public interface IMusicListUI : IMusicUIBase
{
	void Setup(IReadOnlyObservableList<GameAudioInfo> audioInfoList, FacilityMusic facilityMusic);
}
