using Bulbul;
using R3;
using UnityEngine.Scripting;

namespace ES3Types;

[Preserve]
[ES3Properties(new string[] { "value" })]
public class ES3UserType_SerializableSaveDataSyncIntervalReactiveProperty : ES3ObjectType
{
	public static ES3Type Instance;

	public ES3UserType_SerializableSaveDataSyncIntervalReactiveProperty()
		: base(typeof(SerializableReactiveProperty<SaveDataSyncInterval>))
	{
		Instance = this;
		priority = 1;
	}

	protected override void WriteObject(object obj, ES3Writer writer)
	{
		SerializableReactiveProperty<SaveDataSyncInterval> objectContainingField = (SerializableReactiveProperty<SaveDataSyncInterval>)obj;
		writer.WritePrivateField("value", objectContainingField);
	}

	protected override void ReadObject<T>(ES3Reader reader, object obj)
	{
		SerializableReactiveProperty<SaveDataSyncInterval> serializableReactiveProperty = (SerializableReactiveProperty<SaveDataSyncInterval>)obj;
		foreach (string property in reader.Properties)
		{
			if (property == "value")
			{
				serializableReactiveProperty.Value = reader.Read<SaveDataSyncInterval>();
			}
			else
			{
				reader.Skip();
			}
		}
	}

	protected override object ReadObject<T>(ES3Reader reader)
	{
		SerializableReactiveProperty<SaveDataSyncInterval> serializableReactiveProperty = new SerializableReactiveProperty<SaveDataSyncInterval>();
		ReadObject<T>(reader, serializableReactiveProperty);
		return serializableReactiveProperty;
	}
}
