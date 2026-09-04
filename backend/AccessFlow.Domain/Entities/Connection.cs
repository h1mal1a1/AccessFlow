using AccessFlow.Domain.Constants;

namespace AccessFlow.Domain.Entities;

public class Connection
{
    public long Id { get; set; }
    public long IdClient { get; set; }

    /// <summary>
    /// Идентификатор подключения во внешней системе/VPS
    /// </summary>
    public required string IdExternal { get; set; }
    public required string Name { get; set; }
    public required string ConnectionString { get; set; }
    public required string SubUrl { get; set; }
    public required ConnectionStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Client Client { get; set; } = null!;
    public ICollection<Notification> Notifications { get; set; } = [];
}