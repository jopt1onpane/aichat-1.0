using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[JsonSerializable(typeof(FileMeta))]
[JsonSerializable(typeof(FilesSnapshot))]
[JsonSerializable(typeof(AccountData))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, IncludeFields = true)]
[DoNotObfuscateClass]
[GeneratedCode("System.Text.Json.SourceGeneration", "10.0.13.7005")]
public class FileSaveSourceGenerationContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<AccountData>? _AccountData;

	private JsonTypeInfo<FileMeta>? _FileMeta;

	private JsonTypeInfo<FilesSnapshot>? _FilesSnapshot;

	private JsonTypeInfo<List<FileMeta>>? _ListFileMeta;

	private JsonTypeInfo<long>? _Int64;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
	{
		IncludeFields = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly JsonEncodedText PropName_deviceID = JsonEncodedText.Encode("deviceID");

	private static readonly JsonEncodedText PropName_fileName = JsonEncodedText.Encode("fileName");

	private static readonly JsonEncodedText PropName_size = JsonEncodedText.Encode("size");

	private static readonly JsonEncodedText PropName_lastWriteTime = JsonEncodedText.Encode("lastWriteTime");

	private static readonly JsonEncodedText PropName_totalSize = JsonEncodedText.Encode("totalSize");

	private static readonly JsonEncodedText PropName_files = JsonEncodedText.Encode("files");

	public JsonTypeInfo<AccountData> AccountData => _AccountData ?? (_AccountData = (JsonTypeInfo<AccountData>)base.Options.GetTypeInfo(typeof(AccountData)));

	public JsonTypeInfo<FileMeta> FileMeta => _FileMeta ?? (_FileMeta = (JsonTypeInfo<FileMeta>)base.Options.GetTypeInfo(typeof(FileMeta)));

	public JsonTypeInfo<FilesSnapshot> FilesSnapshot => _FilesSnapshot ?? (_FilesSnapshot = (JsonTypeInfo<FilesSnapshot>)base.Options.GetTypeInfo(typeof(FilesSnapshot)));

	public JsonTypeInfo<List<FileMeta>> ListFileMeta => _ListFileMeta ?? (_ListFileMeta = (JsonTypeInfo<List<FileMeta>>)base.Options.GetTypeInfo(typeof(List<FileMeta>)));

	public JsonTypeInfo<long> Int64 => _Int64 ?? (_Int64 = (JsonTypeInfo<long>)base.Options.GetTypeInfo(typeof(long)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static FileSaveSourceGenerationContext Default { get; } = new FileSaveSourceGenerationContext(new JsonSerializerOptions(s_defaultOptions));

	protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

	private JsonTypeInfo<AccountData> Create_AccountData(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<AccountData> jsonTypeInfo))
		{
			JsonObjectInfoValues<AccountData> objectInfo = new JsonObjectInfoValues<AccountData>
			{
				ObjectCreator = () => new AccountData(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => AccountDataPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(AccountData).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = AccountDataSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] AccountDataPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[1];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(AccountData),
			Converter = null,
			Getter = (object obj) => ((AccountData)obj).DeviceID,
			Setter = delegate(object obj, string? value)
			{
				((AccountData)obj).DeviceID = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "DeviceID",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(AccountData).GetField("DeviceID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		return array;
	}

	private void AccountDataSerializeHandler(Utf8JsonWriter writer, AccountData? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_deviceID, value.DeviceID);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<FileMeta> Create_FileMeta(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<FileMeta> jsonTypeInfo))
		{
			JsonObjectInfoValues<FileMeta> objectInfo = new JsonObjectInfoValues<FileMeta>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new FileMeta((string)args[0], (long)args[1], (long)args[2]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => FileMetaPropInit(options),
				ConstructorParameterMetadataInitializer = FileMetaCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(FileMeta).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[3]
				{
					typeof(string),
					typeof(long),
					typeof(long)
				}, null),
				SerializeHandler = FileMetaSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] FileMetaPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileMeta),
			Converter = null,
			Getter = (object obj) => ((FileMeta)obj).FileName,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "FileName",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileMeta).GetField("FileName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<long> propertyInfo2 = new JsonPropertyInfoValues<long>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileMeta),
			Converter = null,
			Getter = (object obj) => ((FileMeta)obj).Size,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Size",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileMeta).GetField("Size", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<long> propertyInfo3 = new JsonPropertyInfoValues<long>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileMeta),
			Converter = null,
			Getter = (object obj) => ((FileMeta)obj).LastWriteTime,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LastWriteTime",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileMeta).GetField("LastWriteTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private void FileMetaSerializeHandler(Utf8JsonWriter writer, FileMeta? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_fileName, value.FileName);
		writer.WriteNumber(PropName_size, value.Size);
		writer.WriteNumber(PropName_lastWriteTime, value.LastWriteTime);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] FileMetaCtorParamInit()
	{
		return new JsonParameterInfoValues[3]
		{
			new JsonParameterInfoValues
			{
				Name = "fileName",
				ParameterType = typeof(string),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "size",
				ParameterType = typeof(long),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "lastWriteTime",
				ParameterType = typeof(long),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<FilesSnapshot> Create_FilesSnapshot(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<FilesSnapshot> jsonTypeInfo))
		{
			JsonObjectInfoValues<FilesSnapshot> objectInfo = new JsonObjectInfoValues<FilesSnapshot>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new FilesSnapshot((List<FileMeta>)args[0], (long)args[1]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => FilesSnapshotPropInit(options),
				ConstructorParameterMetadataInitializer = FilesSnapshotCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(FilesSnapshot).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(List<FileMeta>),
					typeof(long)
				}, null),
				SerializeHandler = FilesSnapshotSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] FilesSnapshotPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<long> propertyInfo = new JsonPropertyInfoValues<long>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FilesSnapshot),
			Converter = null,
			Getter = (object obj) => ((FilesSnapshot)obj).TotalSize,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TotalSize",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FilesSnapshot).GetProperty("TotalSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<List<FileMeta>> propertyInfo2 = new JsonPropertyInfoValues<List<FileMeta>>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FilesSnapshot),
			Converter = null,
			Getter = (object obj) => ((FilesSnapshot)obj).files,
			Setter = delegate(object obj, List<FileMeta>? value)
			{
				((FilesSnapshot)obj).files = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "files",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FilesSnapshot).GetField("files", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private void FilesSnapshotSerializeHandler(Utf8JsonWriter writer, FilesSnapshot? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteNumber(PropName_totalSize, value.TotalSize);
		writer.WritePropertyName(PropName_files);
		ListFileMetaSerializeHandler(writer, value.files);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] FilesSnapshotCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "files",
				ParameterType = typeof(List<FileMeta>),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "totalSize",
				ParameterType = typeof(long),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<List<FileMeta>> Create_ListFileMeta(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<FileMeta>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<FileMeta>> collectionInfo = new JsonCollectionInfoValues<List<FileMeta>>
			{
				ObjectCreator = () => new List<FileMeta>(),
				SerializeHandler = ListFileMetaSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<FileMeta>, FileMeta>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListFileMetaSerializeHandler(Utf8JsonWriter writer, List<FileMeta>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			FileMetaSerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<long> Create_Int64(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<long> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<long>(options, JsonMetadataServices.Int64Converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<string> Create_String(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<string> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<string>(options, JsonMetadataServices.StringConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	public FileSaveSourceGenerationContext()
		: base(null)
	{
	}

	public FileSaveSourceGenerationContext(JsonSerializerOptions options)
		: base(options)
	{
	}

	private static bool TryGetTypeInfoForRuntimeCustomConverter<TJsonMetadataType>(JsonSerializerOptions options, out JsonTypeInfo<TJsonMetadataType> jsonTypeInfo)
	{
		JsonConverter runtimeConverterForType = GetRuntimeConverterForType(typeof(TJsonMetadataType), options);
		if (runtimeConverterForType != null)
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<TJsonMetadataType>(options, runtimeConverterForType);
			return true;
		}
		jsonTypeInfo = null;
		return false;
	}

	private static JsonConverter? GetRuntimeConverterForType(Type type, JsonSerializerOptions options)
	{
		for (int i = 0; i < options.Converters.Count; i++)
		{
			JsonConverter jsonConverter = options.Converters[i];
			if (jsonConverter != null && jsonConverter.CanConvert(type))
			{
				return ExpandConverter(type, jsonConverter, options, validateCanConvert: false);
			}
		}
		return null;
	}

	private static JsonConverter ExpandConverter(Type type, JsonConverter converter, JsonSerializerOptions options, bool validateCanConvert = true)
	{
		if (validateCanConvert && !converter.CanConvert(type))
		{
			throw new InvalidOperationException($"The converter '{converter.GetType()}' is not compatible with the type '{type}'.");
		}
		if (converter is JsonConverterFactory jsonConverterFactory)
		{
			converter = jsonConverterFactory.CreateConverter(type, options);
			if (converter == null || converter is JsonConverterFactory)
			{
				throw new InvalidOperationException($"The converter '{jsonConverterFactory.GetType()}' cannot return null or a JsonConverterFactory instance.");
			}
		}
		return converter;
	}

	public override JsonTypeInfo? GetTypeInfo(Type type)
	{
		base.Options.TryGetTypeInfo(type, out JsonTypeInfo typeInfo);
		return typeInfo;
	}

	JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options)
	{
		if (type == typeof(AccountData))
		{
			return Create_AccountData(options);
		}
		if (type == typeof(FileMeta))
		{
			return Create_FileMeta(options);
		}
		if (type == typeof(FilesSnapshot))
		{
			return Create_FilesSnapshot(options);
		}
		if (type == typeof(List<FileMeta>))
		{
			return Create_ListFileMeta(options);
		}
		if (type == typeof(long))
		{
			return Create_Int64(options);
		}
		if (type == typeof(string))
		{
			return Create_String(options);
		}
		return null;
	}
}
