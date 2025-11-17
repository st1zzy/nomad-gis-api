namespace nomad_gis_V2.DTOs.Messages;

public class MessageRequest
{
    public Guid MapPointId { get; set; }
    public string Content { get; set; } = string.Empty;
}