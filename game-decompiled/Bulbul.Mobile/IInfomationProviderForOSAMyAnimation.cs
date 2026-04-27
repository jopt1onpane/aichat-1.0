namespace Bulbul.Mobile;

public interface IInfomationProviderForOSAMyAnimation
{
	void RequestChangeItemSizeAndUpdateLayout(int idx, float size, bool endEdgeStationary = false);

	void RequestChangeItemSize(int idx, float size);
}
