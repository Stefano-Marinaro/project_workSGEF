using GoCare.Models.Enums;

namespace GoCare.Dtos.Devices.Requests;

public sealed record RegisterDeviceRequest(string PushToken, EDevicePlatform Platform);
