using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Bulbul.CustomJsonConverter;

namespace Bulbul.Web;

[JsonSerializable(typeof(Nil))]
[JsonSerializable(typeof(ErrorCodeResponse))]
[JsonSerializable(typeof(SignupResponse))]
[JsonSerializable(typeof(CheckAccountResponse))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(CheckLoginResponse))]
[JsonSerializable(typeof(StartupCheckResponse))]
[JsonSerializable(typeof(GetSaveDataListResponse))]
[JsonSerializable(typeof(SyncSaveDataResponse))]
[JsonSerializable(typeof(FileDownLoadResponse))]
[JsonSerializable(typeof(PurchaseResponse))]
[JsonSerializable(typeof(GetDeviceNonConsumableResponse))]
[JsonSerializable(typeof(UserStatus))]
[JsonSerializable(typeof(GetCheckPurchaseResponse))]
[JsonSerializable(typeof(GetShopSettingsResponse))]
[JsonSerializable(typeof(GetNewsResponse))]
[JsonSerializable(typeof(NewsData))]
[JsonSerializable(typeof(ShopSetting))]
[JsonSerializable(typeof(ShopMaintenance))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, IncludeFields = true)]
[GeneratedCode("System.Text.Json.SourceGeneration", "10.0.13.7005")]
public class SourceGenerationContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<bool>? _Boolean;

	private JsonTypeInfo<AccountType>? _AccountType;

	private JsonTypeInfo<CheckAccountStatus>? _CheckAccountStatus;

	private JsonTypeInfo<ErrorCode>? _ErrorCode;

	private JsonTypeInfo<CheckAccountResponse>? _CheckAccountResponse;

	private JsonTypeInfo<CheckLoginResponse>? _CheckLoginResponse;

	private JsonTypeInfo<ErrorCodeResponse>? _ErrorCodeResponse;

	private JsonTypeInfo<FileDownLoadResponse>? _FileDownLoadResponse;

	private JsonTypeInfo<GetCheckPurchaseResponse>? _GetCheckPurchaseResponse;

	private JsonTypeInfo<GetDeviceNonConsumableResponse>? _GetDeviceNonConsumableResponse;

	private JsonTypeInfo<GetNewsResponse>? _GetNewsResponse;

	private JsonTypeInfo<GetSaveDataListResponse>? _GetSaveDataListResponse;

	private JsonTypeInfo<GetShopSettingsResponse>? _GetShopSettingsResponse;

	private JsonTypeInfo<LoginResponse>? _LoginResponse;

	private JsonTypeInfo<NewsData>? _NewsData;

	private JsonTypeInfo<NewsData[]>? _NewsDataArray;

	private JsonTypeInfo<Nil>? _Nil;

	private JsonTypeInfo<PurchaseResponse>? _PurchaseResponse;

	private JsonTypeInfo<ShopMaintenance>? _ShopMaintenance;

	private JsonTypeInfo<ShopSetting>? _ShopSetting;

	private JsonTypeInfo<ShopSetting[]>? _ShopSettingArray;

	private JsonTypeInfo<SignupResponse>? _SignupResponse;

	private JsonTypeInfo<StartupCheckResponse>? _StartupCheckResponse;

	private JsonTypeInfo<SyncSaveDataResponse>? _SyncSaveDataResponse;

	private JsonTypeInfo<UnlockProducts>? _UnlockProducts;

	private JsonTypeInfo<UserStatus>? _UserStatus;

	private JsonTypeInfo<List<AccountType>>? _ListAccountType;

	private JsonTypeInfo<DateTime>? _DateTime;

	private JsonTypeInfo<DirectoryInfo>? _DirectoryInfo;

	private JsonTypeInfo<FileAttributes>? _FileAttributes;

	private JsonTypeInfo<FileInfo>? _FileInfo;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<long>? _Int64;

	private JsonTypeInfo<string>? _String;

	private JsonTypeInfo<string[]>? _StringArray;

	private JsonTypeInfo<ulong>? _UInt64;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
	{
		IncludeFields = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly JsonEncodedText PropName_errorCode = JsonEncodedText.Encode("errorCode");

	private static readonly JsonEncodedText PropName_isUserIdLinked = JsonEncodedText.Encode("isUserIdLinked");

	private static readonly JsonEncodedText PropName_status = JsonEncodedText.Encode("status");

	private static readonly JsonEncodedText PropName_linkSessionID = JsonEncodedText.Encode("linkSessionID");

	private static readonly JsonEncodedText PropName_senderUserStatus = JsonEncodedText.Encode("senderUserStatus");

	private static readonly JsonEncodedText PropName_otherUserStatus = JsonEncodedText.Encode("otherUserStatus");

	private static readonly JsonEncodedText PropName_isLogin = JsonEncodedText.Encode("isLogin");

	private static readonly JsonEncodedText PropName_isNewsBadge = JsonEncodedText.Encode("isNewsBadge");

	private static readonly JsonEncodedText PropName_shopMaintenanceInfo = JsonEncodedText.Encode("shopMaintenanceInfo");

	private static readonly JsonEncodedText PropName_fileInfo = JsonEncodedText.Encode("fileInfo");

	private static readonly JsonEncodedText PropName_isPossible = JsonEncodedText.Encode("isPossible");

	private static readonly JsonEncodedText PropName_reason = JsonEncodedText.Encode("reason");

	private static readonly JsonEncodedText PropName_unlockProducts = JsonEncodedText.Encode("unlockProducts");

	private static readonly JsonEncodedText PropName_newsDatas = JsonEncodedText.Encode("newsDatas");

	private static readonly JsonEncodedText PropName_list = JsonEncodedText.Encode("list");

	private static readonly JsonEncodedText PropName_shopSettings = JsonEncodedText.Encode("shopSettings");

	private static readonly JsonEncodedText PropName_userID = JsonEncodedText.Encode("userID");

	private static readonly JsonEncodedText PropName_linkedAccounts = JsonEncodedText.Encode("linkedAccounts");

	private static readonly JsonEncodedText PropName_isReview = JsonEncodedText.Encode("isReview");

	private static readonly JsonEncodedText PropName_isGranted = JsonEncodedText.Encode("isGranted");

	private static readonly JsonEncodedText PropName_transactionId = JsonEncodedText.Encode("transactionId");

	private static readonly JsonEncodedText PropName_deviceID = JsonEncodedText.Encode("deviceID");

	private static readonly JsonEncodedText PropName_isMaintenance = JsonEncodedText.Encode("isMaintenance");

	private static readonly JsonEncodedText PropName_maintenanceStart = JsonEncodedText.Encode("maintenanceStart");

	private static readonly JsonEncodedText PropName_maintenanceEnd = JsonEncodedText.Encode("maintenanceEnd");

	private static readonly JsonEncodedText PropName_mainText = JsonEncodedText.Encode("mainText");

	private static readonly JsonEncodedText PropName_isUpdateApp = JsonEncodedText.Encode("isUpdateApp");

	private static readonly JsonEncodedText PropName_isConsentRequired = JsonEncodedText.Encode("isConsentRequired");

	private static readonly JsonEncodedText PropName_isDeleteUser = JsonEncodedText.Encode("isDeleteUser");

	private static readonly JsonEncodedText PropName_sessionID = JsonEncodedText.Encode("sessionID");

	private static readonly JsonEncodedText PropName_productIds = JsonEncodedText.Encode("productIds");

	private static readonly JsonEncodedText PropName_level = JsonEncodedText.Encode("level");

	private static readonly JsonEncodedText PropName_pomodoroSeconds = JsonEncodedText.Encode("pomodoroSeconds");

	private static readonly JsonEncodedText PropName_lastSaveDate = JsonEncodedText.Encode("lastSaveDate");

	private static readonly JsonEncodedText PropName_exists = JsonEncodedText.Encode("exists");

	private static readonly JsonEncodedText PropName_name = JsonEncodedText.Encode("name");

	private static readonly JsonEncodedText PropName_parent = JsonEncodedText.Encode("parent");

	private static readonly JsonEncodedText PropName_root = JsonEncodedText.Encode("root");

	private static readonly JsonEncodedText PropName_attributes = JsonEncodedText.Encode("attributes");

	private static readonly JsonEncodedText PropName_creationTime = JsonEncodedText.Encode("creationTime");

	private static readonly JsonEncodedText PropName_creationTimeUtc = JsonEncodedText.Encode("creationTimeUtc");

	private static readonly JsonEncodedText PropName_extension = JsonEncodedText.Encode("extension");

	private static readonly JsonEncodedText PropName_fullName = JsonEncodedText.Encode("fullName");

	private static readonly JsonEncodedText PropName_lastAccessTime = JsonEncodedText.Encode("lastAccessTime");

	private static readonly JsonEncodedText PropName_lastAccessTimeUtc = JsonEncodedText.Encode("lastAccessTimeUtc");

	private static readonly JsonEncodedText PropName_lastWriteTime = JsonEncodedText.Encode("lastWriteTime");

	private static readonly JsonEncodedText PropName_lastWriteTimeUtc = JsonEncodedText.Encode("lastWriteTimeUtc");

	private static readonly JsonEncodedText PropName_directory = JsonEncodedText.Encode("directory");

	private static readonly JsonEncodedText PropName_directoryName = JsonEncodedText.Encode("directoryName");

	private static readonly JsonEncodedText PropName_isReadOnly = JsonEncodedText.Encode("isReadOnly");

	private static readonly JsonEncodedText PropName_length = JsonEncodedText.Encode("length");

	public JsonTypeInfo<bool> Boolean => _Boolean ?? (_Boolean = (JsonTypeInfo<bool>)base.Options.GetTypeInfo(typeof(bool)));

	public JsonTypeInfo<AccountType> AccountType => _AccountType ?? (_AccountType = (JsonTypeInfo<AccountType>)base.Options.GetTypeInfo(typeof(AccountType)));

	public JsonTypeInfo<CheckAccountStatus> CheckAccountStatus => _CheckAccountStatus ?? (_CheckAccountStatus = (JsonTypeInfo<CheckAccountStatus>)base.Options.GetTypeInfo(typeof(CheckAccountStatus)));

	public JsonTypeInfo<ErrorCode> ErrorCode => _ErrorCode ?? (_ErrorCode = (JsonTypeInfo<ErrorCode>)base.Options.GetTypeInfo(typeof(ErrorCode)));

	public JsonTypeInfo<CheckAccountResponse> CheckAccountResponse => _CheckAccountResponse ?? (_CheckAccountResponse = (JsonTypeInfo<CheckAccountResponse>)base.Options.GetTypeInfo(typeof(CheckAccountResponse)));

	public JsonTypeInfo<CheckLoginResponse> CheckLoginResponse => _CheckLoginResponse ?? (_CheckLoginResponse = (JsonTypeInfo<CheckLoginResponse>)base.Options.GetTypeInfo(typeof(CheckLoginResponse)));

	public JsonTypeInfo<ErrorCodeResponse> ErrorCodeResponse => _ErrorCodeResponse ?? (_ErrorCodeResponse = (JsonTypeInfo<ErrorCodeResponse>)base.Options.GetTypeInfo(typeof(ErrorCodeResponse)));

	public JsonTypeInfo<FileDownLoadResponse> FileDownLoadResponse => _FileDownLoadResponse ?? (_FileDownLoadResponse = (JsonTypeInfo<FileDownLoadResponse>)base.Options.GetTypeInfo(typeof(FileDownLoadResponse)));

	public JsonTypeInfo<GetCheckPurchaseResponse> GetCheckPurchaseResponse => _GetCheckPurchaseResponse ?? (_GetCheckPurchaseResponse = (JsonTypeInfo<GetCheckPurchaseResponse>)base.Options.GetTypeInfo(typeof(GetCheckPurchaseResponse)));

	public JsonTypeInfo<GetDeviceNonConsumableResponse> GetDeviceNonConsumableResponse => _GetDeviceNonConsumableResponse ?? (_GetDeviceNonConsumableResponse = (JsonTypeInfo<GetDeviceNonConsumableResponse>)base.Options.GetTypeInfo(typeof(GetDeviceNonConsumableResponse)));

	public JsonTypeInfo<GetNewsResponse> GetNewsResponse => _GetNewsResponse ?? (_GetNewsResponse = (JsonTypeInfo<GetNewsResponse>)base.Options.GetTypeInfo(typeof(GetNewsResponse)));

	public JsonTypeInfo<GetSaveDataListResponse> GetSaveDataListResponse => _GetSaveDataListResponse ?? (_GetSaveDataListResponse = (JsonTypeInfo<GetSaveDataListResponse>)base.Options.GetTypeInfo(typeof(GetSaveDataListResponse)));

	public JsonTypeInfo<GetShopSettingsResponse> GetShopSettingsResponse => _GetShopSettingsResponse ?? (_GetShopSettingsResponse = (JsonTypeInfo<GetShopSettingsResponse>)base.Options.GetTypeInfo(typeof(GetShopSettingsResponse)));

	public JsonTypeInfo<LoginResponse> LoginResponse => _LoginResponse ?? (_LoginResponse = (JsonTypeInfo<LoginResponse>)base.Options.GetTypeInfo(typeof(LoginResponse)));

	public JsonTypeInfo<NewsData> NewsData => _NewsData ?? (_NewsData = (JsonTypeInfo<NewsData>)base.Options.GetTypeInfo(typeof(NewsData)));

	public JsonTypeInfo<NewsData[]> NewsDataArray => _NewsDataArray ?? (_NewsDataArray = (JsonTypeInfo<NewsData[]>)base.Options.GetTypeInfo(typeof(NewsData[])));

	public JsonTypeInfo<Nil> Nil => _Nil ?? (_Nil = (JsonTypeInfo<Nil>)base.Options.GetTypeInfo(typeof(Nil)));

	public JsonTypeInfo<PurchaseResponse> PurchaseResponse => _PurchaseResponse ?? (_PurchaseResponse = (JsonTypeInfo<PurchaseResponse>)base.Options.GetTypeInfo(typeof(PurchaseResponse)));

	public JsonTypeInfo<ShopMaintenance> ShopMaintenance => _ShopMaintenance ?? (_ShopMaintenance = (JsonTypeInfo<ShopMaintenance>)base.Options.GetTypeInfo(typeof(ShopMaintenance)));

	public JsonTypeInfo<ShopSetting> ShopSetting => _ShopSetting ?? (_ShopSetting = (JsonTypeInfo<ShopSetting>)base.Options.GetTypeInfo(typeof(ShopSetting)));

	public JsonTypeInfo<ShopSetting[]> ShopSettingArray => _ShopSettingArray ?? (_ShopSettingArray = (JsonTypeInfo<ShopSetting[]>)base.Options.GetTypeInfo(typeof(ShopSetting[])));

	public JsonTypeInfo<SignupResponse> SignupResponse => _SignupResponse ?? (_SignupResponse = (JsonTypeInfo<SignupResponse>)base.Options.GetTypeInfo(typeof(SignupResponse)));

	public JsonTypeInfo<StartupCheckResponse> StartupCheckResponse => _StartupCheckResponse ?? (_StartupCheckResponse = (JsonTypeInfo<StartupCheckResponse>)base.Options.GetTypeInfo(typeof(StartupCheckResponse)));

	public JsonTypeInfo<SyncSaveDataResponse> SyncSaveDataResponse => _SyncSaveDataResponse ?? (_SyncSaveDataResponse = (JsonTypeInfo<SyncSaveDataResponse>)base.Options.GetTypeInfo(typeof(SyncSaveDataResponse)));

	public JsonTypeInfo<UnlockProducts> UnlockProducts => _UnlockProducts ?? (_UnlockProducts = (JsonTypeInfo<UnlockProducts>)base.Options.GetTypeInfo(typeof(UnlockProducts)));

	public JsonTypeInfo<UserStatus> UserStatus => _UserStatus ?? (_UserStatus = (JsonTypeInfo<UserStatus>)base.Options.GetTypeInfo(typeof(UserStatus)));

	public JsonTypeInfo<List<AccountType>> ListAccountType => _ListAccountType ?? (_ListAccountType = (JsonTypeInfo<List<AccountType>>)base.Options.GetTypeInfo(typeof(List<AccountType>)));

	public JsonTypeInfo<DateTime> DateTime => _DateTime ?? (_DateTime = (JsonTypeInfo<DateTime>)base.Options.GetTypeInfo(typeof(DateTime)));

	public JsonTypeInfo<DirectoryInfo> DirectoryInfo => _DirectoryInfo ?? (_DirectoryInfo = (JsonTypeInfo<DirectoryInfo>)base.Options.GetTypeInfo(typeof(DirectoryInfo)));

	public JsonTypeInfo<FileAttributes> FileAttributes => _FileAttributes ?? (_FileAttributes = (JsonTypeInfo<FileAttributes>)base.Options.GetTypeInfo(typeof(FileAttributes)));

	public JsonTypeInfo<FileInfo> FileInfo => _FileInfo ?? (_FileInfo = (JsonTypeInfo<FileInfo>)base.Options.GetTypeInfo(typeof(FileInfo)));

	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	public JsonTypeInfo<long> Int64 => _Int64 ?? (_Int64 = (JsonTypeInfo<long>)base.Options.GetTypeInfo(typeof(long)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public JsonTypeInfo<string[]> StringArray => _StringArray ?? (_StringArray = (JsonTypeInfo<string[]>)base.Options.GetTypeInfo(typeof(string[])));

	public JsonTypeInfo<ulong> UInt64 => _UInt64 ?? (_UInt64 = (JsonTypeInfo<ulong>)base.Options.GetTypeInfo(typeof(ulong)));

	public static SourceGenerationContext Default { get; } = new SourceGenerationContext(new JsonSerializerOptions(s_defaultOptions));

	protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

	private JsonTypeInfo<bool> Create_Boolean(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<bool> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<bool>(options, JsonMetadataServices.BooleanConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<AccountType> Create_AccountType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<AccountType> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<AccountType>(options, JsonMetadataServices.GetEnumConverter<AccountType>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<CheckAccountStatus> Create_CheckAccountStatus(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CheckAccountStatus> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<CheckAccountStatus>(options, JsonMetadataServices.GetEnumConverter<CheckAccountStatus>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<ErrorCode> Create_ErrorCode(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ErrorCode> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<ErrorCode>(options, JsonMetadataServices.GetEnumConverter<ErrorCode>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<CheckAccountResponse> Create_CheckAccountResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CheckAccountResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<CheckAccountResponse> objectInfo = new JsonObjectInfoValues<CheckAccountResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new CheckAccountResponse((bool)args[0], (CheckAccountStatus)args[1], (string)args[2], (UserStatus)args[3], (UserStatus)args[4], (ErrorCode)args[5]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => CheckAccountResponsePropInit(options),
				ConstructorParameterMetadataInitializer = CheckAccountResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(CheckAccountResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[6]
				{
					typeof(bool),
					typeof(CheckAccountStatus),
					typeof(string),
					typeof(UserStatus),
					typeof(UserStatus),
					typeof(ErrorCode)
				}, null),
				SerializeHandler = CheckAccountResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] CheckAccountResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[6];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(CheckAccountResponse),
			Converter = null,
			Getter = (object obj) => ((CheckAccountResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(CheckAccountResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<bool> propertyInfo2 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(CheckAccountResponse),
			Converter = null,
			Getter = (object obj) => ((CheckAccountResponse)obj).IsUserIdLinked,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsUserIdLinked",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(CheckAccountResponse).GetField("IsUserIdLinked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<CheckAccountStatus> propertyInfo3 = new JsonPropertyInfoValues<CheckAccountStatus>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(CheckAccountResponse),
			Converter = null,
			Getter = (object obj) => ((CheckAccountResponse)obj).Status,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Status",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(CheckAccountResponse).GetField("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(CheckAccountResponse),
			Converter = null,
			Getter = (object obj) => ((CheckAccountResponse)obj).LinkSessionID,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LinkSessionID",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(CheckAccountResponse).GetField("LinkSessionID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<UserStatus> propertyInfo5 = new JsonPropertyInfoValues<UserStatus>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(CheckAccountResponse),
			Converter = null,
			Getter = (object obj) => ((CheckAccountResponse)obj).SenderUserStatus,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "SenderUserStatus",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(CheckAccountResponse).GetField("SenderUserStatus", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<UserStatus> propertyInfo6 = new JsonPropertyInfoValues<UserStatus>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(CheckAccountResponse),
			Converter = null,
			Getter = (object obj) => ((CheckAccountResponse)obj).OtherUserStatus,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "OtherUserStatus",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(CheckAccountResponse).GetField("OtherUserStatus", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		return array;
	}

	private void CheckAccountResponseSerializeHandler(Utf8JsonWriter writer, CheckAccountResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		CheckAccountResponse checkAccountResponse = value;
		JsonSerializer.Serialize(writer, checkAccountResponse.ErrorCode, ErrorCode);
		writer.WriteBoolean(PropName_isUserIdLinked, value.IsUserIdLinked);
		writer.WritePropertyName(PropName_status);
		JsonSerializer.Serialize(writer, value.Status, CheckAccountStatus);
		writer.WriteString(PropName_linkSessionID, value.LinkSessionID);
		writer.WritePropertyName(PropName_senderUserStatus);
		UserStatusSerializeHandler(writer, value.SenderUserStatus);
		writer.WritePropertyName(PropName_otherUserStatus);
		UserStatusSerializeHandler(writer, value.OtherUserStatus);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] CheckAccountResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[6]
		{
			new JsonParameterInfoValues
			{
				Name = "isUserIdLinked",
				ParameterType = typeof(bool),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "status",
				ParameterType = typeof(CheckAccountStatus),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "linkSessionID",
				ParameterType = typeof(string),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "senderUserStatus",
				ParameterType = typeof(UserStatus),
				Position = 3,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "otherUserStatus",
				ParameterType = typeof(UserStatus),
				Position = 4,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 5,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<CheckLoginResponse> Create_CheckLoginResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CheckLoginResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<CheckLoginResponse> objectInfo = new JsonObjectInfoValues<CheckLoginResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new CheckLoginResponse((bool)args[0], (bool)args[1], (ShopMaintenance)args[2], (ErrorCode)args[3]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => CheckLoginResponsePropInit(options),
				ConstructorParameterMetadataInitializer = CheckLoginResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(CheckLoginResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[4]
				{
					typeof(bool),
					typeof(bool),
					typeof(ShopMaintenance),
					typeof(ErrorCode)
				}, null),
				SerializeHandler = CheckLoginResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] CheckLoginResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(CheckLoginResponse),
			Converter = null,
			Getter = (object obj) => ((CheckLoginResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(CheckLoginResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<bool> propertyInfo2 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(CheckLoginResponse),
			Converter = null,
			Getter = (object obj) => ((CheckLoginResponse)obj).IsLogin,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsLogin",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(CheckLoginResponse).GetField("IsLogin", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<bool> propertyInfo3 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(CheckLoginResponse),
			Converter = null,
			Getter = (object obj) => ((CheckLoginResponse)obj).IsNewsBadge,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsNewsBadge",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(CheckLoginResponse).GetField("IsNewsBadge", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<ShopMaintenance> propertyInfo4 = new JsonPropertyInfoValues<ShopMaintenance>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(CheckLoginResponse),
			Converter = null,
			Getter = (object obj) => ((CheckLoginResponse)obj).ShopMaintenanceInfo,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ShopMaintenanceInfo",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(CheckLoginResponse).GetField("ShopMaintenanceInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private void CheckLoginResponseSerializeHandler(Utf8JsonWriter writer, CheckLoginResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		CheckLoginResponse checkLoginResponse = value;
		JsonSerializer.Serialize(writer, checkLoginResponse.ErrorCode, ErrorCode);
		writer.WriteBoolean(PropName_isLogin, value.IsLogin);
		writer.WriteBoolean(PropName_isNewsBadge, value.IsNewsBadge);
		writer.WritePropertyName(PropName_shopMaintenanceInfo);
		JsonSerializer.Serialize(writer, value.ShopMaintenanceInfo, ShopMaintenance);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] CheckLoginResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[4]
		{
			new JsonParameterInfoValues
			{
				Name = "isLogin",
				ParameterType = typeof(bool),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "isNewsBadge",
				ParameterType = typeof(bool),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "shopMaintenanceInfo",
				ParameterType = typeof(ShopMaintenance),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 3,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<ErrorCodeResponse> Create_ErrorCodeResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ErrorCodeResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<ErrorCodeResponse> objectInfo = new JsonObjectInfoValues<ErrorCodeResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new ErrorCodeResponse((ErrorCode)args[0]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => ErrorCodeResponsePropInit(options),
				ConstructorParameterMetadataInitializer = ErrorCodeResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(ErrorCodeResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(ErrorCode) }, null),
				SerializeHandler = ErrorCodeResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ErrorCodeResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[1];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ErrorCodeResponse),
			Converter = null,
			Getter = (object obj) => ((ErrorCodeResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ErrorCodeResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		return array;
	}

	private void ErrorCodeResponseSerializeHandler(Utf8JsonWriter writer, ErrorCodeResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		ErrorCodeResponse errorCodeResponse = value;
		JsonSerializer.Serialize(writer, errorCodeResponse.ErrorCode, ErrorCode);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] ErrorCodeResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[1]
		{
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<FileDownLoadResponse> Create_FileDownLoadResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<FileDownLoadResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<FileDownLoadResponse> objectInfo = new JsonObjectInfoValues<FileDownLoadResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new FileDownLoadResponse((FileInfo)args[0], (ErrorCode)args[1]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => FileDownLoadResponsePropInit(options),
				ConstructorParameterMetadataInitializer = FileDownLoadResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(FileDownLoadResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(FileInfo),
					typeof(ErrorCode)
				}, null),
				SerializeHandler = FileDownLoadResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] FileDownLoadResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileDownLoadResponse),
			Converter = null,
			Getter = (object obj) => ((FileDownLoadResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileDownLoadResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<FileInfo> propertyInfo2 = new JsonPropertyInfoValues<FileInfo>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileDownLoadResponse),
			Converter = null,
			Getter = (object obj) => ((FileDownLoadResponse)obj).FileInfo,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "FileInfo",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileDownLoadResponse).GetField("FileInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private void FileDownLoadResponseSerializeHandler(Utf8JsonWriter writer, FileDownLoadResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		FileDownLoadResponse fileDownLoadResponse = value;
		JsonSerializer.Serialize(writer, fileDownLoadResponse.ErrorCode, ErrorCode);
		writer.WritePropertyName(PropName_fileInfo);
		FileInfoSerializeHandler(writer, value.FileInfo);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] FileDownLoadResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "fileInfo",
				ParameterType = typeof(FileInfo),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<GetCheckPurchaseResponse> Create_GetCheckPurchaseResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<GetCheckPurchaseResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<GetCheckPurchaseResponse> objectInfo = new JsonObjectInfoValues<GetCheckPurchaseResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new GetCheckPurchaseResponse((ErrorCode)args[0], (bool)args[1], (int)args[2], (ShopMaintenance)args[3]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => GetCheckPurchaseResponsePropInit(options),
				ConstructorParameterMetadataInitializer = GetCheckPurchaseResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(GetCheckPurchaseResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[4]
				{
					typeof(ErrorCode),
					typeof(bool),
					typeof(int),
					typeof(ShopMaintenance)
				}, null),
				SerializeHandler = GetCheckPurchaseResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] GetCheckPurchaseResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetCheckPurchaseResponse),
			Converter = null,
			Getter = (object obj) => ((GetCheckPurchaseResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetCheckPurchaseResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<bool> propertyInfo2 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetCheckPurchaseResponse),
			Converter = null,
			Getter = (object obj) => ((GetCheckPurchaseResponse)obj).IsPossible,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsPossible",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetCheckPurchaseResponse).GetProperty("IsPossible", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<ShopMaintenance> propertyInfo3 = new JsonPropertyInfoValues<ShopMaintenance>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetCheckPurchaseResponse),
			Converter = null,
			Getter = (object obj) => ((GetCheckPurchaseResponse)obj).ShopMaintenanceInfo,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ShopMaintenanceInfo",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetCheckPurchaseResponse).GetProperty("ShopMaintenanceInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ShopMaintenance), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<int> propertyInfo4 = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetCheckPurchaseResponse),
			Converter = null,
			Getter = (object obj) => ((GetCheckPurchaseResponse)obj).Reason,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Reason",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetCheckPurchaseResponse).GetProperty("Reason", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private void GetCheckPurchaseResponseSerializeHandler(Utf8JsonWriter writer, GetCheckPurchaseResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		GetCheckPurchaseResponse getCheckPurchaseResponse = value;
		JsonSerializer.Serialize(writer, getCheckPurchaseResponse.ErrorCode, ErrorCode);
		JsonEncodedText propName_isPossible = PropName_isPossible;
		getCheckPurchaseResponse = value;
		writer.WriteBoolean(propName_isPossible, getCheckPurchaseResponse.IsPossible);
		writer.WritePropertyName(PropName_shopMaintenanceInfo);
		getCheckPurchaseResponse = value;
		JsonSerializer.Serialize(writer, getCheckPurchaseResponse.ShopMaintenanceInfo, ShopMaintenance);
		JsonEncodedText propName_reason = PropName_reason;
		getCheckPurchaseResponse = value;
		writer.WriteNumber(propName_reason, getCheckPurchaseResponse.Reason);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] GetCheckPurchaseResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[4]
		{
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "isPossible",
				ParameterType = typeof(bool),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "reason",
				ParameterType = typeof(int),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "shopMaintenanceInfo",
				ParameterType = typeof(ShopMaintenance),
				Position = 3,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<GetDeviceNonConsumableResponse> Create_GetDeviceNonConsumableResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<GetDeviceNonConsumableResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<GetDeviceNonConsumableResponse> objectInfo = new JsonObjectInfoValues<GetDeviceNonConsumableResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new GetDeviceNonConsumableResponse((ErrorCode)args[0], (UnlockProducts)args[1]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => GetDeviceNonConsumableResponsePropInit(options),
				ConstructorParameterMetadataInitializer = GetDeviceNonConsumableResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(GetDeviceNonConsumableResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(ErrorCode),
					typeof(UnlockProducts)
				}, null),
				SerializeHandler = GetDeviceNonConsumableResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] GetDeviceNonConsumableResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetDeviceNonConsumableResponse),
			Converter = null,
			Getter = (object obj) => ((GetDeviceNonConsumableResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetDeviceNonConsumableResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<UnlockProducts> propertyInfo2 = new JsonPropertyInfoValues<UnlockProducts>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetDeviceNonConsumableResponse),
			Converter = null,
			Getter = (object obj) => ((GetDeviceNonConsumableResponse)obj).UnlockProducts,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "UnlockProducts",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetDeviceNonConsumableResponse).GetField("UnlockProducts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private void GetDeviceNonConsumableResponseSerializeHandler(Utf8JsonWriter writer, GetDeviceNonConsumableResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		GetDeviceNonConsumableResponse getDeviceNonConsumableResponse = value;
		JsonSerializer.Serialize(writer, getDeviceNonConsumableResponse.ErrorCode, ErrorCode);
		writer.WritePropertyName(PropName_unlockProducts);
		UnlockProductsSerializeHandler(writer, value.UnlockProducts);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] GetDeviceNonConsumableResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "unlockProducts",
				ParameterType = typeof(UnlockProducts),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			}
		};
	}

	private JsonTypeInfo<GetNewsResponse> Create_GetNewsResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<GetNewsResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<GetNewsResponse> objectInfo = new JsonObjectInfoValues<GetNewsResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new GetNewsResponse((NewsData[])args[0], (ErrorCode)args[1]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => GetNewsResponsePropInit(options),
				ConstructorParameterMetadataInitializer = GetNewsResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(GetNewsResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(NewsData[]),
					typeof(ErrorCode)
				}, null),
				SerializeHandler = GetNewsResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] GetNewsResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<NewsData[]> propertyInfo = new JsonPropertyInfoValues<NewsData[]>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetNewsResponse),
			Converter = null,
			Getter = (object obj) => ((GetNewsResponse)obj).NewsDatas,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "NewsDatas",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetNewsResponse).GetProperty("NewsDatas", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(NewsData[]), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<ErrorCode> propertyInfo2 = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetNewsResponse),
			Converter = null,
			Getter = (object obj) => ((GetNewsResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetNewsResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private void GetNewsResponseSerializeHandler(Utf8JsonWriter writer, GetNewsResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_newsDatas);
		GetNewsResponse getNewsResponse = value;
		NewsDataArraySerializeHandler(writer, getNewsResponse.NewsDatas);
		writer.WritePropertyName(PropName_errorCode);
		getNewsResponse = value;
		JsonSerializer.Serialize(writer, getNewsResponse.ErrorCode, ErrorCode);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] GetNewsResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "newsDatas",
				ParameterType = typeof(NewsData[]),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<GetSaveDataListResponse> Create_GetSaveDataListResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<GetSaveDataListResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<GetSaveDataListResponse> objectInfo = new JsonObjectInfoValues<GetSaveDataListResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new GetSaveDataListResponse((string[])args[0], (ErrorCode)args[1]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => GetSaveDataListResponsePropInit(options),
				ConstructorParameterMetadataInitializer = GetSaveDataListResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(GetSaveDataListResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(string[]),
					typeof(ErrorCode)
				}, null),
				SerializeHandler = GetSaveDataListResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] GetSaveDataListResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetSaveDataListResponse),
			Converter = null,
			Getter = (object obj) => ((GetSaveDataListResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetSaveDataListResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string[]> propertyInfo2 = new JsonPropertyInfoValues<string[]>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetSaveDataListResponse),
			Converter = null,
			Getter = (object obj) => ((GetSaveDataListResponse)obj).List,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "List",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetSaveDataListResponse).GetField("List", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private void GetSaveDataListResponseSerializeHandler(Utf8JsonWriter writer, GetSaveDataListResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		GetSaveDataListResponse getSaveDataListResponse = value;
		JsonSerializer.Serialize(writer, getSaveDataListResponse.ErrorCode, ErrorCode);
		writer.WritePropertyName(PropName_list);
		StringArraySerializeHandler(writer, value.List);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] GetSaveDataListResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "list",
				ParameterType = typeof(string[]),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<GetShopSettingsResponse> Create_GetShopSettingsResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<GetShopSettingsResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<GetShopSettingsResponse> objectInfo = new JsonObjectInfoValues<GetShopSettingsResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new GetShopSettingsResponse((ShopSetting[])args[0], (ErrorCode)args[1]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => GetShopSettingsResponsePropInit(options),
				ConstructorParameterMetadataInitializer = GetShopSettingsResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(GetShopSettingsResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(ShopSetting[]),
					typeof(ErrorCode)
				}, null),
				SerializeHandler = GetShopSettingsResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] GetShopSettingsResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetShopSettingsResponse),
			Converter = null,
			Getter = (object obj) => ((GetShopSettingsResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetShopSettingsResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<ShopSetting[]> propertyInfo2 = new JsonPropertyInfoValues<ShopSetting[]>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(GetShopSettingsResponse),
			Converter = null,
			Getter = (object obj) => ((GetShopSettingsResponse)obj).ShopSettings,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ShopSettings",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(GetShopSettingsResponse).GetField("ShopSettings", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private void GetShopSettingsResponseSerializeHandler(Utf8JsonWriter writer, GetShopSettingsResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		GetShopSettingsResponse getShopSettingsResponse = value;
		JsonSerializer.Serialize(writer, getShopSettingsResponse.ErrorCode, ErrorCode);
		writer.WritePropertyName(PropName_shopSettings);
		ShopSettingArraySerializeHandler(writer, value.ShopSettings);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] GetShopSettingsResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "shopSettings",
				ParameterType = typeof(ShopSetting[]),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<LoginResponse> Create_LoginResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<LoginResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<LoginResponse> objectInfo = new JsonObjectInfoValues<LoginResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new LoginResponse((int)args[0], (List<AccountType>)args[1], (UnlockProducts)args[2], (bool)args[3], (ShopMaintenance)args[4], (ErrorCode)args[5]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => LoginResponsePropInit(options),
				ConstructorParameterMetadataInitializer = LoginResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(LoginResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[6]
				{
					typeof(int),
					typeof(List<AccountType>),
					typeof(UnlockProducts),
					typeof(bool),
					typeof(ShopMaintenance),
					typeof(ErrorCode)
				}, null),
				SerializeHandler = LoginResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] LoginResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[6];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(LoginResponse),
			Converter = null,
			Getter = (object obj) => ((LoginResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(LoginResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<int> propertyInfo2 = new JsonPropertyInfoValues<int>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(LoginResponse),
			Converter = null,
			Getter = (object obj) => ((LoginResponse)obj).UserID,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "UserID",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(LoginResponse).GetField("UserID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<List<AccountType>> propertyInfo3 = new JsonPropertyInfoValues<List<AccountType>>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(LoginResponse),
			Converter = null,
			Getter = (object obj) => ((LoginResponse)obj).LinkedAccounts,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LinkedAccounts",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(LoginResponse).GetField("LinkedAccounts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<UnlockProducts> propertyInfo4 = new JsonPropertyInfoValues<UnlockProducts>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(LoginResponse),
			Converter = null,
			Getter = (object obj) => ((LoginResponse)obj).UnlockProducts,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "UnlockProducts",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(LoginResponse).GetField("UnlockProducts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<bool> propertyInfo5 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(LoginResponse),
			Converter = null,
			Getter = (object obj) => ((LoginResponse)obj).IsReview,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsReview",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(LoginResponse).GetField("IsReview", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<ShopMaintenance> propertyInfo6 = new JsonPropertyInfoValues<ShopMaintenance>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(LoginResponse),
			Converter = null,
			Getter = (object obj) => ((LoginResponse)obj).ShopMaintenanceInfo,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ShopMaintenanceInfo",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(LoginResponse).GetField("ShopMaintenanceInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		return array;
	}

	private void LoginResponseSerializeHandler(Utf8JsonWriter writer, LoginResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		LoginResponse loginResponse = value;
		JsonSerializer.Serialize(writer, loginResponse.ErrorCode, ErrorCode);
		writer.WriteNumber(PropName_userID, value.UserID);
		writer.WritePropertyName(PropName_linkedAccounts);
		ListAccountTypeSerializeHandler(writer, value.LinkedAccounts);
		writer.WritePropertyName(PropName_unlockProducts);
		UnlockProductsSerializeHandler(writer, value.UnlockProducts);
		writer.WriteBoolean(PropName_isReview, value.IsReview);
		writer.WritePropertyName(PropName_shopMaintenanceInfo);
		JsonSerializer.Serialize(writer, value.ShopMaintenanceInfo, ShopMaintenance);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] LoginResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[6]
		{
			new JsonParameterInfoValues
			{
				Name = "userID",
				ParameterType = typeof(int),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "linkedAccounts",
				ParameterType = typeof(List<AccountType>),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "unlockProducts",
				ParameterType = typeof(UnlockProducts),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "isReview",
				ParameterType = typeof(bool),
				Position = 3,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "shopMaintenanceInfo",
				ParameterType = typeof(ShopMaintenance),
				Position = 4,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 5,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<NewsData> Create_NewsData(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<NewsData> jsonTypeInfo))
		{
			JsonObjectInfoValues<NewsData> objectInfo = new JsonObjectInfoValues<NewsData>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new NewsData((ulong)args[0], (DateTime)args[1], (DateTime)args[2], (string)args[3], (string)args[4]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => NewsDataPropInit(options),
				ConstructorParameterMetadataInitializer = NewsDataCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(NewsData).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[5]
				{
					typeof(ulong),
					typeof(DateTime),
					typeof(DateTime),
					typeof(string),
					typeof(string)
				}, null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] NewsDataPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<ulong> propertyInfo = new JsonPropertyInfoValues<ulong>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(NewsData),
			Converter = null,
			Getter = (object obj) => ((NewsData)obj).ID,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ID",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(NewsData).GetField("ID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<DateTime> propertyInfo2 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(NewsData),
			Converter = (JsonConverter<DateTime>)ExpandConverter(typeof(DateTime), new DateTimeConverterParseExactBulbulFormat(), options),
			Getter = (object obj) => ((NewsData)obj).StartDate,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "StartDate",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(NewsData).GetField("StartDate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<DateTime> propertyInfo3 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(NewsData),
			Converter = (JsonConverter<DateTime>)ExpandConverter(typeof(DateTime), new DateTimeConverterParseExactBulbulFormat(), options),
			Getter = (object obj) => ((NewsData)obj).EndDate,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "EndDate",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(NewsData).GetField("EndDate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(NewsData),
			Converter = null,
			Getter = (object obj) => ((NewsData)obj).Title,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Title",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(NewsData).GetField("Title", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<string> propertyInfo5 = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(NewsData),
			Converter = null,
			Getter = (object obj) => ((NewsData)obj).MainText,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "MainText",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(NewsData).GetField("MainText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		return array;
	}

	private static JsonParameterInfoValues[] NewsDataCtorParamInit()
	{
		return new JsonParameterInfoValues[5]
		{
			new JsonParameterInfoValues
			{
				Name = "id",
				ParameterType = typeof(ulong),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "startDate",
				ParameterType = typeof(DateTime),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "endDate",
				ParameterType = typeof(DateTime),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "title",
				ParameterType = typeof(string),
				Position = 3,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "mainText",
				ParameterType = typeof(string),
				Position = 4,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			}
		};
	}

	private JsonTypeInfo<NewsData[]> Create_NewsDataArray(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<NewsData[]> jsonTypeInfo))
		{
			JsonCollectionInfoValues<NewsData[]> collectionInfo = new JsonCollectionInfoValues<NewsData[]>
			{
				ObjectCreator = null,
				SerializeHandler = NewsDataArraySerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateArrayInfo(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void NewsDataArraySerializeHandler(Utf8JsonWriter writer, NewsData[]? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Length; i++)
		{
			JsonSerializer.Serialize(writer, value[i], NewsData);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<Nil> Create_Nil(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Nil> jsonTypeInfo))
		{
			JsonObjectInfoValues<Nil> objectInfo = new JsonObjectInfoValues<Nil>
			{
				ObjectCreator = () => default(Nil),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => NilPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(Nil).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = NilSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] NilPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[1];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(Nil),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(Nil).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		return array;
	}

	private void NilSerializeHandler(Utf8JsonWriter writer, Nil value)
	{
		writer.WriteStartObject();
		writer.WriteEndObject();
	}

	private JsonTypeInfo<PurchaseResponse> Create_PurchaseResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<PurchaseResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<PurchaseResponse> objectInfo = new JsonObjectInfoValues<PurchaseResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new PurchaseResponse((ErrorCode)args[0], (bool)args[1], (string)args[2]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => PurchaseResponsePropInit(options),
				ConstructorParameterMetadataInitializer = PurchaseResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(PurchaseResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[3]
				{
					typeof(ErrorCode),
					typeof(bool),
					typeof(string)
				}, null),
				SerializeHandler = PurchaseResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] PurchaseResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(PurchaseResponse),
			Converter = null,
			Getter = (object obj) => ((PurchaseResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(PurchaseResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<bool> propertyInfo2 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(PurchaseResponse),
			Converter = null,
			Getter = (object obj) => ((PurchaseResponse)obj).IsGranted,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsGranted",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(PurchaseResponse).GetProperty("IsGranted", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(PurchaseResponse),
			Converter = null,
			Getter = (object obj) => ((PurchaseResponse)obj).TransactionId,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TransactionId",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(PurchaseResponse).GetProperty("TransactionId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private void PurchaseResponseSerializeHandler(Utf8JsonWriter writer, PurchaseResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		PurchaseResponse purchaseResponse = value;
		JsonSerializer.Serialize(writer, purchaseResponse.ErrorCode, ErrorCode);
		JsonEncodedText propName_isGranted = PropName_isGranted;
		purchaseResponse = value;
		writer.WriteBoolean(propName_isGranted, purchaseResponse.IsGranted);
		JsonEncodedText propName_transactionId = PropName_transactionId;
		purchaseResponse = value;
		writer.WriteString(propName_transactionId, purchaseResponse.TransactionId);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] PurchaseResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[3]
		{
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "isGranted",
				ParameterType = typeof(bool),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "transactionId",
				ParameterType = typeof(string),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			}
		};
	}

	private JsonTypeInfo<ShopMaintenance> Create_ShopMaintenance(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ShopMaintenance> jsonTypeInfo))
		{
			JsonObjectInfoValues<ShopMaintenance> objectInfo = new JsonObjectInfoValues<ShopMaintenance>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new ShopMaintenance((bool)args[0], (DateTime)args[1], (DateTime)args[2], (string)args[3]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => ShopMaintenancePropInit(options),
				ConstructorParameterMetadataInitializer = ShopMaintenanceCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(ShopMaintenance).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[4]
				{
					typeof(bool),
					typeof(DateTime),
					typeof(DateTime),
					typeof(string)
				}, null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ShopMaintenancePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<bool> propertyInfo = new JsonPropertyInfoValues<bool>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ShopMaintenance),
			Converter = null,
			Getter = (object obj) => ((ShopMaintenance)obj).IsMaintenance,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsMaintenance",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ShopMaintenance).GetField("IsMaintenance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<DateTime> propertyInfo2 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ShopMaintenance),
			Converter = (JsonConverter<DateTime>)ExpandConverter(typeof(DateTime), new DateTimeConverterParseExactBulbulFormat(), options),
			Getter = (object obj) => ((ShopMaintenance)obj).StartDate,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "StartDate",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ShopMaintenance).GetField("StartDate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<DateTime> propertyInfo3 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ShopMaintenance),
			Converter = (JsonConverter<DateTime>)ExpandConverter(typeof(DateTime), new DateTimeConverterParseExactBulbulFormat(), options),
			Getter = (object obj) => ((ShopMaintenance)obj).EndDate,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "EndDate",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ShopMaintenance).GetField("EndDate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ShopMaintenance),
			Converter = null,
			Getter = (object obj) => ((ShopMaintenance)obj).MainText,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "MainText",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ShopMaintenance).GetField("MainText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private static JsonParameterInfoValues[] ShopMaintenanceCtorParamInit()
	{
		return new JsonParameterInfoValues[4]
		{
			new JsonParameterInfoValues
			{
				Name = "isMaintenance",
				ParameterType = typeof(bool),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "startDate",
				ParameterType = typeof(DateTime),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "endDate",
				ParameterType = typeof(DateTime),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "mainText",
				ParameterType = typeof(string),
				Position = 3,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			}
		};
	}

	private JsonTypeInfo<ShopSetting> Create_ShopSetting(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ShopSetting> jsonTypeInfo))
		{
			JsonObjectInfoValues<ShopSetting> objectInfo = new JsonObjectInfoValues<ShopSetting>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new ShopSetting((string)args[0], (DateTime)args[1], (DateTime)args[2]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => ShopSettingPropInit(options),
				ConstructorParameterMetadataInitializer = ShopSettingCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(ShopSetting).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[3]
				{
					typeof(string),
					typeof(DateTime),
					typeof(DateTime)
				}, null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ShopSettingPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ShopSetting),
			Converter = null,
			Getter = (object obj) => ((ShopSetting)obj).ProductId,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ProductId",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ShopSetting).GetField("ProductId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<DateTime> propertyInfo2 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ShopSetting),
			Converter = (JsonConverter<DateTime>)ExpandConverter(typeof(DateTime), new DateTimeConverterParseExactBulbulFormat(), options),
			Getter = (object obj) => ((ShopSetting)obj).StartDate,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "StartDate",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ShopSetting).GetField("StartDate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<DateTime> propertyInfo3 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ShopSetting),
			Converter = (JsonConverter<DateTime>)ExpandConverter(typeof(DateTime), new DateTimeConverterParseExactBulbulFormat(), options),
			Getter = (object obj) => ((ShopSetting)obj).EndDate,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "EndDate",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ShopSetting).GetField("EndDate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private static JsonParameterInfoValues[] ShopSettingCtorParamInit()
	{
		return new JsonParameterInfoValues[3]
		{
			new JsonParameterInfoValues
			{
				Name = "productId",
				ParameterType = typeof(string),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "startDate",
				ParameterType = typeof(DateTime),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "endDate",
				ParameterType = typeof(DateTime),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<ShopSetting[]> Create_ShopSettingArray(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ShopSetting[]> jsonTypeInfo))
		{
			JsonCollectionInfoValues<ShopSetting[]> collectionInfo = new JsonCollectionInfoValues<ShopSetting[]>
			{
				ObjectCreator = null,
				SerializeHandler = ShopSettingArraySerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateArrayInfo(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ShopSettingArraySerializeHandler(Utf8JsonWriter writer, ShopSetting[]? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Length; i++)
		{
			JsonSerializer.Serialize(writer, value[i], ShopSetting);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<SignupResponse> Create_SignupResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SignupResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<SignupResponse> objectInfo = new JsonObjectInfoValues<SignupResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new SignupResponse((int)args[0], (string)args[1], (ErrorCode)args[2]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => SignupResponsePropInit(options),
				ConstructorParameterMetadataInitializer = SignupResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(SignupResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[3]
				{
					typeof(int),
					typeof(string),
					typeof(ErrorCode)
				}, null),
				SerializeHandler = SignupResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SignupResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SignupResponse),
			Converter = null,
			Getter = (object obj) => ((SignupResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SignupResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<int> propertyInfo2 = new JsonPropertyInfoValues<int>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SignupResponse),
			Converter = null,
			Getter = (object obj) => ((SignupResponse)obj).UserID,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "UserID",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SignupResponse).GetField("UserID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SignupResponse),
			Converter = null,
			Getter = (object obj) => ((SignupResponse)obj).DeviceID,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "DeviceID",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SignupResponse).GetField("DeviceID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private void SignupResponseSerializeHandler(Utf8JsonWriter writer, SignupResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		SignupResponse signupResponse = value;
		JsonSerializer.Serialize(writer, signupResponse.ErrorCode, ErrorCode);
		writer.WriteNumber(PropName_userID, value.UserID);
		writer.WriteString(PropName_deviceID, value.DeviceID);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] SignupResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[3]
		{
			new JsonParameterInfoValues
			{
				Name = "userID",
				ParameterType = typeof(int),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "deviceID",
				ParameterType = typeof(string),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<StartupCheckResponse> Create_StartupCheckResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<StartupCheckResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<StartupCheckResponse> objectInfo = new JsonObjectInfoValues<StartupCheckResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new StartupCheckResponse((bool)args[0], (string)args[1], (string)args[2], (string)args[3], (bool)args[4], (bool)args[5], (bool)args[6], (ErrorCode)args[7]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => StartupCheckResponsePropInit(options),
				ConstructorParameterMetadataInitializer = StartupCheckResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(StartupCheckResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[8]
				{
					typeof(bool),
					typeof(string),
					typeof(string),
					typeof(string),
					typeof(bool),
					typeof(bool),
					typeof(bool),
					typeof(ErrorCode)
				}, null),
				SerializeHandler = StartupCheckResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] StartupCheckResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[8];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(StartupCheckResponse),
			Converter = null,
			Getter = (object obj) => ((StartupCheckResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(StartupCheckResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<bool> propertyInfo2 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(StartupCheckResponse),
			Converter = null,
			Getter = (object obj) => ((StartupCheckResponse)obj).IsMaintenance,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsMaintenance",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(StartupCheckResponse).GetField("IsMaintenance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(StartupCheckResponse),
			Converter = null,
			Getter = (object obj) => ((StartupCheckResponse)obj).MaintenanceStart,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "MaintenanceStart",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(StartupCheckResponse).GetField("MaintenanceStart", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(StartupCheckResponse),
			Converter = null,
			Getter = (object obj) => ((StartupCheckResponse)obj).MaintenanceEnd,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "MaintenanceEnd",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(StartupCheckResponse).GetField("MaintenanceEnd", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<string> propertyInfo5 = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(StartupCheckResponse),
			Converter = null,
			Getter = (object obj) => ((StartupCheckResponse)obj).MainText,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "MainText",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(StartupCheckResponse).GetField("MainText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<bool> propertyInfo6 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(StartupCheckResponse),
			Converter = null,
			Getter = (object obj) => ((StartupCheckResponse)obj).IsUpdateApp,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsUpdateApp",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(StartupCheckResponse).GetField("IsUpdateApp", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		JsonPropertyInfoValues<bool> propertyInfo7 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(StartupCheckResponse),
			Converter = null,
			Getter = (object obj) => ((StartupCheckResponse)obj).IsConsentRequired,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsConsentRequired",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(StartupCheckResponse).GetField("IsConsentRequired", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		JsonPropertyInfoValues<bool> propertyInfo8 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(StartupCheckResponse),
			Converter = null,
			Getter = (object obj) => ((StartupCheckResponse)obj).IsDeleteUser,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsDeleteUser",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(StartupCheckResponse).GetField("IsDeleteUser", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		return array;
	}

	private void StartupCheckResponseSerializeHandler(Utf8JsonWriter writer, StartupCheckResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		StartupCheckResponse startupCheckResponse = value;
		JsonSerializer.Serialize(writer, startupCheckResponse.ErrorCode, ErrorCode);
		writer.WriteBoolean(PropName_isMaintenance, value.IsMaintenance);
		writer.WriteString(PropName_maintenanceStart, value.MaintenanceStart);
		writer.WriteString(PropName_maintenanceEnd, value.MaintenanceEnd);
		writer.WriteString(PropName_mainText, value.MainText);
		writer.WriteBoolean(PropName_isUpdateApp, value.IsUpdateApp);
		writer.WriteBoolean(PropName_isConsentRequired, value.IsConsentRequired);
		writer.WriteBoolean(PropName_isDeleteUser, value.IsDeleteUser);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] StartupCheckResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[8]
		{
			new JsonParameterInfoValues
			{
				Name = "isMaintenance",
				ParameterType = typeof(bool),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "maintenanceStart",
				ParameterType = typeof(string),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "maintenanceEnd",
				ParameterType = typeof(string),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "mainText",
				ParameterType = typeof(string),
				Position = 3,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "isUpdateApp",
				ParameterType = typeof(bool),
				Position = 4,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "isConsentRequired",
				ParameterType = typeof(bool),
				Position = 5,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "isDeleteUser",
				ParameterType = typeof(bool),
				Position = 6,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 7,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<SyncSaveDataResponse> Create_SyncSaveDataResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SyncSaveDataResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<SyncSaveDataResponse> objectInfo = new JsonObjectInfoValues<SyncSaveDataResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new SyncSaveDataResponse((string)args[0], (ErrorCode)args[1]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => SyncSaveDataResponsePropInit(options),
				ConstructorParameterMetadataInitializer = SyncSaveDataResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(SyncSaveDataResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(string),
					typeof(ErrorCode)
				}, null),
				SerializeHandler = SyncSaveDataResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SyncSaveDataResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<ErrorCode> propertyInfo = new JsonPropertyInfoValues<ErrorCode>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SyncSaveDataResponse),
			Converter = null,
			Getter = (object obj) => ((SyncSaveDataResponse)obj).ErrorCode,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ErrorCode",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SyncSaveDataResponse).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ErrorCode), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SyncSaveDataResponse),
			Converter = null,
			Getter = (object obj) => ((SyncSaveDataResponse)obj).SessionID,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "SessionID",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SyncSaveDataResponse).GetField("SessionID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private void SyncSaveDataResponseSerializeHandler(Utf8JsonWriter writer, SyncSaveDataResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_errorCode);
		SyncSaveDataResponse syncSaveDataResponse = value;
		JsonSerializer.Serialize(writer, syncSaveDataResponse.ErrorCode, ErrorCode);
		writer.WriteString(PropName_sessionID, value.SessionID);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] SyncSaveDataResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "sessionID",
				ParameterType = typeof(string),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			},
			new JsonParameterInfoValues
			{
				Name = "errorCode",
				ParameterType = typeof(ErrorCode),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<UnlockProducts> Create_UnlockProducts(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<UnlockProducts> jsonTypeInfo))
		{
			JsonObjectInfoValues<UnlockProducts> objectInfo = new JsonObjectInfoValues<UnlockProducts>
			{
				ObjectCreator = () => new UnlockProducts(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => UnlockProductsPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(UnlockProducts).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = UnlockProductsSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] UnlockProductsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[1];
		JsonPropertyInfoValues<string[]> propertyInfo = new JsonPropertyInfoValues<string[]>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(UnlockProducts),
			Converter = null,
			Getter = (object obj) => ((UnlockProducts)obj).productIds,
			Setter = delegate(object obj, string[]? value)
			{
				((UnlockProducts)obj).productIds = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "productIds",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(UnlockProducts).GetField("productIds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		return array;
	}

	private void UnlockProductsSerializeHandler(Utf8JsonWriter writer, UnlockProducts? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_productIds);
		StringArraySerializeHandler(writer, value.productIds);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<UserStatus> Create_UserStatus(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<UserStatus> jsonTypeInfo))
		{
			JsonObjectInfoValues<UserStatus> objectInfo = new JsonObjectInfoValues<UserStatus>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new UserStatus((int)args[0], (int)args[1], (string)args[2]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => UserStatusPropInit(options),
				ConstructorParameterMetadataInitializer = UserStatusCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(UserStatus).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[3]
				{
					typeof(int),
					typeof(int),
					typeof(string)
				}, null),
				SerializeHandler = UserStatusSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] UserStatusPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<int> propertyInfo = new JsonPropertyInfoValues<int>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(UserStatus),
			Converter = null,
			Getter = (object obj) => ((UserStatus)obj).Level,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Level",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(UserStatus).GetField("Level", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<int> propertyInfo2 = new JsonPropertyInfoValues<int>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(UserStatus),
			Converter = null,
			Getter = (object obj) => ((UserStatus)obj).PomodoroSeconds,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "PomodoroSeconds",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(UserStatus).GetField("PomodoroSeconds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(UserStatus),
			Converter = null,
			Getter = (object obj) => ((UserStatus)obj).LastSaveDate,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LastSaveDate",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(UserStatus).GetField("LastSaveDate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private void UserStatusSerializeHandler(Utf8JsonWriter writer, UserStatus? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteNumber(PropName_level, value.Level);
		writer.WriteNumber(PropName_pomodoroSeconds, value.PomodoroSeconds);
		writer.WriteString(PropName_lastSaveDate, value.LastSaveDate);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] UserStatusCtorParamInit()
	{
		return new JsonParameterInfoValues[3]
		{
			new JsonParameterInfoValues
			{
				Name = "level",
				ParameterType = typeof(int),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "pomodoroSeconds",
				ParameterType = typeof(int),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "lastSaveDate",
				ParameterType = typeof(string),
				Position = 2,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			}
		};
	}

	private JsonTypeInfo<List<AccountType>> Create_ListAccountType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<AccountType>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<AccountType>> collectionInfo = new JsonCollectionInfoValues<List<AccountType>>
			{
				ObjectCreator = () => new List<AccountType>(),
				SerializeHandler = ListAccountTypeSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<AccountType>, AccountType>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListAccountTypeSerializeHandler(Utf8JsonWriter writer, List<AccountType>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			JsonSerializer.Serialize(writer, value[i], AccountType);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<DateTime> Create_DateTime(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<DateTime> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<DateTime>(options, JsonMetadataServices.DateTimeConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<DirectoryInfo> Create_DirectoryInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<DirectoryInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<DirectoryInfo> objectInfo = new JsonObjectInfoValues<DirectoryInfo>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new DirectoryInfo((string)args[0]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => DirectoryInfoPropInit(options),
				ConstructorParameterMetadataInitializer = DirectoryInfoCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(DirectoryInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(string) }, null),
				SerializeHandler = DirectoryInfoSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] DirectoryInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[15];
		JsonPropertyInfoValues<bool> propertyInfo = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = true,
			DeclaringType = typeof(DirectoryInfo),
			Converter = null,
			Getter = (object obj) => ((DirectoryInfo)obj).Exists,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Exists",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(DirectoryInfo).GetProperty("Exists", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = true,
			DeclaringType = typeof(DirectoryInfo),
			Converter = null,
			Getter = (object obj) => ((DirectoryInfo)obj).Name,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Name",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(DirectoryInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<DirectoryInfo> propertyInfo3 = new JsonPropertyInfoValues<DirectoryInfo>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(DirectoryInfo),
			Converter = null,
			Getter = (object obj) => ((DirectoryInfo)obj).Parent,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Parent",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(DirectoryInfo).GetProperty("Parent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DirectoryInfo), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<DirectoryInfo> propertyInfo4 = new JsonPropertyInfoValues<DirectoryInfo>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(DirectoryInfo),
			Converter = null,
			Getter = (object obj) => ((DirectoryInfo)obj).Root,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Root",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(DirectoryInfo).GetProperty("Root", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DirectoryInfo), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<FileAttributes> propertyInfo5 = new JsonPropertyInfoValues<FileAttributes>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).Attributes,
			Setter = delegate(object obj, FileAttributes value)
			{
				((FileSystemInfo)obj).Attributes = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Attributes",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("Attributes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(FileAttributes), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<DateTime> propertyInfo6 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).CreationTime,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).CreationTime = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "CreationTime",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("CreationTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		JsonPropertyInfoValues<DateTime> propertyInfo7 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).CreationTimeUtc,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).CreationTimeUtc = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "CreationTimeUtc",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("CreationTimeUtc", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		JsonPropertyInfoValues<bool> propertyInfo8 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = true,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).Exists,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Exists",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("Exists", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		JsonPropertyInfoValues<string> propertyInfo9 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).Extension,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Extension",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("Extension", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		JsonPropertyInfoValues<string> propertyInfo10 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = true,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).FullName,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "FullName",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("FullName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		JsonPropertyInfoValues<DateTime> propertyInfo11 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).LastAccessTime,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).LastAccessTime = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LastAccessTime",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("LastAccessTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		JsonPropertyInfoValues<DateTime> propertyInfo12 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).LastAccessTimeUtc,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).LastAccessTimeUtc = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LastAccessTimeUtc",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("LastAccessTimeUtc", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		JsonPropertyInfoValues<DateTime> propertyInfo13 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).LastWriteTime,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).LastWriteTime = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LastWriteTime",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("LastWriteTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		JsonPropertyInfoValues<DateTime> propertyInfo14 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).LastWriteTimeUtc,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).LastWriteTimeUtc = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LastWriteTimeUtc",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("LastWriteTimeUtc", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		JsonPropertyInfoValues<string> propertyInfo15 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = true,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).Name,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Name",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		return array;
	}

	private void DirectoryInfoSerializeHandler(Utf8JsonWriter writer, DirectoryInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteBoolean(PropName_exists, value.Exists);
		writer.WriteString(PropName_name, value.Name);
		writer.WritePropertyName(PropName_parent);
		DirectoryInfoSerializeHandler(writer, value.Parent);
		writer.WritePropertyName(PropName_root);
		DirectoryInfoSerializeHandler(writer, value.Root);
		writer.WritePropertyName(PropName_attributes);
		JsonSerializer.Serialize(writer, value.Attributes, FileAttributes);
		writer.WriteString(PropName_creationTime, value.CreationTime);
		writer.WriteString(PropName_creationTimeUtc, value.CreationTimeUtc);
		writer.WriteString(PropName_extension, value.Extension);
		writer.WriteString(PropName_fullName, value.FullName);
		writer.WriteString(PropName_lastAccessTime, value.LastAccessTime);
		writer.WriteString(PropName_lastAccessTimeUtc, value.LastAccessTimeUtc);
		writer.WriteString(PropName_lastWriteTime, value.LastWriteTime);
		writer.WriteString(PropName_lastWriteTimeUtc, value.LastWriteTimeUtc);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] DirectoryInfoCtorParamInit()
	{
		return new JsonParameterInfoValues[1]
		{
			new JsonParameterInfoValues
			{
				Name = "path",
				ParameterType = typeof(string),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			}
		};
	}

	private JsonTypeInfo<FileAttributes> Create_FileAttributes(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<FileAttributes> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<FileAttributes>(options, JsonMetadataServices.GetEnumConverter<FileAttributes>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<FileInfo> Create_FileInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<FileInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<FileInfo> objectInfo = new JsonObjectInfoValues<FileInfo>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new FileInfo((string)args[0]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => FileInfoPropInit(options),
				ConstructorParameterMetadataInitializer = FileInfoCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(FileInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(string) }, null),
				SerializeHandler = FileInfoSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] FileInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[17];
		JsonPropertyInfoValues<DirectoryInfo> propertyInfo = new JsonPropertyInfoValues<DirectoryInfo>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileInfo),
			Converter = null,
			Getter = (object obj) => ((FileInfo)obj).Directory,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Directory",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileInfo).GetProperty("Directory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DirectoryInfo), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileInfo),
			Converter = null,
			Getter = (object obj) => ((FileInfo)obj).DirectoryName,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "DirectoryName",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileInfo).GetProperty("DirectoryName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<bool> propertyInfo3 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = true,
			DeclaringType = typeof(FileInfo),
			Converter = null,
			Getter = (object obj) => ((FileInfo)obj).Exists,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Exists",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileInfo).GetProperty("Exists", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<bool> propertyInfo4 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileInfo),
			Converter = null,
			Getter = (object obj) => ((FileInfo)obj).IsReadOnly,
			Setter = delegate(object obj, bool value)
			{
				((FileInfo)obj).IsReadOnly = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "IsReadOnly",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileInfo).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<long> propertyInfo5 = new JsonPropertyInfoValues<long>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileInfo),
			Converter = null,
			Getter = (object obj) => ((FileInfo)obj).Length,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Length",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileInfo).GetProperty("Length", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<string> propertyInfo6 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = true,
			DeclaringType = typeof(FileInfo),
			Converter = null,
			Getter = (object obj) => ((FileInfo)obj).Name,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Name",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		JsonPropertyInfoValues<FileAttributes> propertyInfo7 = new JsonPropertyInfoValues<FileAttributes>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).Attributes,
			Setter = delegate(object obj, FileAttributes value)
			{
				((FileSystemInfo)obj).Attributes = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Attributes",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("Attributes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(FileAttributes), Array.Empty<Type>(), null)
		};
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		JsonPropertyInfoValues<DateTime> propertyInfo8 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).CreationTime,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).CreationTime = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "CreationTime",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("CreationTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		JsonPropertyInfoValues<DateTime> propertyInfo9 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).CreationTimeUtc,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).CreationTimeUtc = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "CreationTimeUtc",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("CreationTimeUtc", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		JsonPropertyInfoValues<bool> propertyInfo10 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = true,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).Exists,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Exists",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("Exists", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		JsonPropertyInfoValues<string> propertyInfo11 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).Extension,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Extension",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("Extension", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		JsonPropertyInfoValues<string> propertyInfo12 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = true,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).FullName,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "FullName",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("FullName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		JsonPropertyInfoValues<DateTime> propertyInfo13 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).LastAccessTime,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).LastAccessTime = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LastAccessTime",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("LastAccessTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		JsonPropertyInfoValues<DateTime> propertyInfo14 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).LastAccessTimeUtc,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).LastAccessTimeUtc = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LastAccessTimeUtc",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("LastAccessTimeUtc", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		JsonPropertyInfoValues<DateTime> propertyInfo15 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).LastWriteTime,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).LastWriteTime = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LastWriteTime",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("LastWriteTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		JsonPropertyInfoValues<DateTime> propertyInfo16 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).LastWriteTimeUtc,
			Setter = delegate(object obj, DateTime value)
			{
				((FileSystemInfo)obj).LastWriteTimeUtc = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LastWriteTimeUtc",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("LastWriteTimeUtc", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[15] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo16);
		JsonPropertyInfoValues<string> propertyInfo17 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = true,
			DeclaringType = typeof(FileSystemInfo),
			Converter = null,
			Getter = (object obj) => ((FileSystemInfo)obj).Name,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Name",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(FileSystemInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[16] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo17);
		return array;
	}

	private void FileInfoSerializeHandler(Utf8JsonWriter writer, FileInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_directory);
		DirectoryInfoSerializeHandler(writer, value.Directory);
		writer.WriteString(PropName_directoryName, value.DirectoryName);
		writer.WriteBoolean(PropName_exists, value.Exists);
		writer.WriteBoolean(PropName_isReadOnly, value.IsReadOnly);
		writer.WriteNumber(PropName_length, value.Length);
		writer.WriteString(PropName_name, value.Name);
		writer.WritePropertyName(PropName_attributes);
		JsonSerializer.Serialize(writer, value.Attributes, FileAttributes);
		writer.WriteString(PropName_creationTime, value.CreationTime);
		writer.WriteString(PropName_creationTimeUtc, value.CreationTimeUtc);
		writer.WriteString(PropName_extension, value.Extension);
		writer.WriteString(PropName_fullName, value.FullName);
		writer.WriteString(PropName_lastAccessTime, value.LastAccessTime);
		writer.WriteString(PropName_lastAccessTimeUtc, value.LastAccessTimeUtc);
		writer.WriteString(PropName_lastWriteTime, value.LastWriteTime);
		writer.WriteString(PropName_lastWriteTimeUtc, value.LastWriteTimeUtc);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] FileInfoCtorParamInit()
	{
		return new JsonParameterInfoValues[1]
		{
			new JsonParameterInfoValues
			{
				Name = "fileName",
				ParameterType = typeof(string),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = true
			}
		};
	}

	private JsonTypeInfo<int> Create_Int32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<int> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<int>(options, JsonMetadataServices.Int32Converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
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

	private JsonTypeInfo<string[]> Create_StringArray(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<string[]> jsonTypeInfo))
		{
			JsonCollectionInfoValues<string[]> collectionInfo = new JsonCollectionInfoValues<string[]>
			{
				ObjectCreator = null,
				SerializeHandler = StringArraySerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateArrayInfo(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void StringArraySerializeHandler(Utf8JsonWriter writer, string[]? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Length; i++)
		{
			writer.WriteStringValue(value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<ulong> Create_UInt64(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ulong> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<ulong>(options, JsonMetadataServices.UInt64Converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	public SourceGenerationContext()
		: base(null)
	{
	}

	public SourceGenerationContext(JsonSerializerOptions options)
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
		if (type == typeof(bool))
		{
			return Create_Boolean(options);
		}
		if (type == typeof(AccountType))
		{
			return Create_AccountType(options);
		}
		if (type == typeof(CheckAccountStatus))
		{
			return Create_CheckAccountStatus(options);
		}
		if (type == typeof(ErrorCode))
		{
			return Create_ErrorCode(options);
		}
		if (type == typeof(CheckAccountResponse))
		{
			return Create_CheckAccountResponse(options);
		}
		if (type == typeof(CheckLoginResponse))
		{
			return Create_CheckLoginResponse(options);
		}
		if (type == typeof(ErrorCodeResponse))
		{
			return Create_ErrorCodeResponse(options);
		}
		if (type == typeof(FileDownLoadResponse))
		{
			return Create_FileDownLoadResponse(options);
		}
		if (type == typeof(GetCheckPurchaseResponse))
		{
			return Create_GetCheckPurchaseResponse(options);
		}
		if (type == typeof(GetDeviceNonConsumableResponse))
		{
			return Create_GetDeviceNonConsumableResponse(options);
		}
		if (type == typeof(GetNewsResponse))
		{
			return Create_GetNewsResponse(options);
		}
		if (type == typeof(GetSaveDataListResponse))
		{
			return Create_GetSaveDataListResponse(options);
		}
		if (type == typeof(GetShopSettingsResponse))
		{
			return Create_GetShopSettingsResponse(options);
		}
		if (type == typeof(LoginResponse))
		{
			return Create_LoginResponse(options);
		}
		if (type == typeof(NewsData))
		{
			return Create_NewsData(options);
		}
		if (type == typeof(NewsData[]))
		{
			return Create_NewsDataArray(options);
		}
		if (type == typeof(Nil))
		{
			return Create_Nil(options);
		}
		if (type == typeof(PurchaseResponse))
		{
			return Create_PurchaseResponse(options);
		}
		if (type == typeof(ShopMaintenance))
		{
			return Create_ShopMaintenance(options);
		}
		if (type == typeof(ShopSetting))
		{
			return Create_ShopSetting(options);
		}
		if (type == typeof(ShopSetting[]))
		{
			return Create_ShopSettingArray(options);
		}
		if (type == typeof(SignupResponse))
		{
			return Create_SignupResponse(options);
		}
		if (type == typeof(StartupCheckResponse))
		{
			return Create_StartupCheckResponse(options);
		}
		if (type == typeof(SyncSaveDataResponse))
		{
			return Create_SyncSaveDataResponse(options);
		}
		if (type == typeof(UnlockProducts))
		{
			return Create_UnlockProducts(options);
		}
		if (type == typeof(UserStatus))
		{
			return Create_UserStatus(options);
		}
		if (type == typeof(List<AccountType>))
		{
			return Create_ListAccountType(options);
		}
		if (type == typeof(DateTime))
		{
			return Create_DateTime(options);
		}
		if (type == typeof(DirectoryInfo))
		{
			return Create_DirectoryInfo(options);
		}
		if (type == typeof(FileAttributes))
		{
			return Create_FileAttributes(options);
		}
		if (type == typeof(FileInfo))
		{
			return Create_FileInfo(options);
		}
		if (type == typeof(int))
		{
			return Create_Int32(options);
		}
		if (type == typeof(long))
		{
			return Create_Int64(options);
		}
		if (type == typeof(string))
		{
			return Create_String(options);
		}
		if (type == typeof(string[]))
		{
			return Create_StringArray(options);
		}
		if (type == typeof(ulong))
		{
			return Create_UInt64(options);
		}
		return null;
	}
}
