using R3;

namespace ES3Types;

public class ES3UserType_SerializableBoolReactivePropertyArray : ES3ArrayType
{
	public static ES3Type Instance;

	public ES3UserType_SerializableBoolReactivePropertyArray()
		: base(typeof(SerializableReactiveProperty<bool>[]), ES3UserType_SerializableBoolReactiveProperty.Instance)
	{
		Instance = this;
	}
}
