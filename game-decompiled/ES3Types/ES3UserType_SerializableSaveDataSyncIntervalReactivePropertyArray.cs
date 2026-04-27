using Bulbul;
using R3;

namespace ES3Types;

public class ES3UserType_SerializableSaveDataSyncIntervalReactivePropertyArray : ES3ArrayType
{
	public static ES3Type Instance;

	public ES3UserType_SerializableSaveDataSyncIntervalReactivePropertyArray()
		: base(typeof(SerializableReactiveProperty<SaveDataSyncInterval>[]), ES3UserType_SerializableSaveDataSyncIntervalReactiveProperty.Instance)
	{
		Instance = this;
	}
}
