namespace AuthServer.Core.Configuration;

public class Client
{
    public Guid Id { get; set; }
    public string Secret { get; set; }
    public List<string> Audiences { get; set; }
}
