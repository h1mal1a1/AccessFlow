namespace AccessFlow.Infrastructure.Vps.Models;

public class ThreeXUiResponse<T>
{
    public required bool Success { get; set; }
    public required string Msg { get; set; }
    public T? Obj { get; set; }
}