using System;

namespace Bulbul;

[Flags]
public enum ResetReason
{
	None = 0,
	DeviceIDInvalid = 1,
	UserIDInvalid = 2,
	Ban = 4,
	Maintenance = 8,
	AppForceUpdate = 0x10,
	TermsUpdate = 0x20,
	MasterDataUpdate = 0x40,
	OtherDeviceLogin = 0x80
}
