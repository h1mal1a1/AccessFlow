namespace AccessFlow.Infrastructure.Vps.Models;

public sealed record ThreeXUiResponse<T>(bool Success, string Msg, T? Obj);