using ApplicationCore.Shared.Data;
using ApplicationCore.Shared.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.Json;

namespace ApplicationCore.Shared.Services;

public class EmailService : IEmailService {

    private readonly string _settingsFilePath;
    private readonly IFileReader _fileReader;
    private Email? _emailSettings = null;

    public EmailService(IOptions<ConfigurationFiles> options, IFileReader fileReader) {
        _settingsFilePath = options.Value.EmailConfigFile;
        _fileReader = fileReader;
    }

    public async Task<MailboxAddress> GetSenderAsync() {
        var settings = await GetSettingsAsync();
        return new(settings.SenderName, settings.SenderEmail);
    }

    public async Task<string> SendMessageAsync(MimeMessage message) {

        var settings = await GetSettingsAsync();

        using var client = new SmtpClient();
        client.Connect(settings.Host, settings.Port, SecureSocketOptions.Auto);
        client.Authenticate(settings.SenderEmail, UserDataProtection.Unprotect(settings.ProtectedPassword));

        var response = await client.SendAsync(message);

        await client.DisconnectAsync(true);

        return response;

    }

    private async Task<Email> GetSettingsAsync() {

        if (_emailSettings is not null) return _emailSettings;

        using var stream = _fileReader.OpenReadFileStream(_settingsFilePath);

        var data = await JsonSerializer.DeserializeAsync<Email>(stream);

        if (data is null) {
            throw new InvalidOperationException("Failed to load email settings");
        }

        return data;

    }

}
