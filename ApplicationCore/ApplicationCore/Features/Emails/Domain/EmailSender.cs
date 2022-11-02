namespace ApplicationCore.Features.Emails.Domain;

public record EmailSender {

    public string Name { get; init; }

    public string Email { get; init; }

    public string Password { get; init; }

    public string Host { get; init; }

    public int Port { get; init; }

    public EmailSender(string name, string email, string password, string host, int port) {
        Name = name;
        Email = email;
        Password = password;
        Host = host;
        Port = port;
    }

}