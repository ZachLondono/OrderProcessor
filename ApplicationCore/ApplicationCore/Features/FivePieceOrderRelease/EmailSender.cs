using ApplicationCore.Shared.Services;
using Domain.Extensions;
using Microsoft.Office.Interop.Outlook;
using MimeKit;
using System.Runtime.InteropServices;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;

namespace ApplicationCore.Features.FivePieceOrderRelease;

public class EmailSender {

    public required string Recipients { get; init; }
    public required string[] Attachments { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public required IEmailService EmailService { get; init; }

    public async Task<bool> SendEmailAsync() {

        var message = new MimeMessage();

        Recipients.Split(';')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ForEach(r => message.To.Add(new MailboxAddress(r, r)));

        if (message.To.Count == 0) {
            return false;
        }

        var sender = EmailService.GetSender();
        message.From.Add(sender);
        message.Subject = Subject;

        var builder = new BodyBuilder {
            TextBody = Body,
        };
        Attachments.Where(File.Exists).ForEach(att => builder.Attachments.Add(att));

        message.Body = builder.ToMessageBody();

        var response = await Task.Run(() => EmailService.SendMessageAsync(message));

        return true;

    }

    public void PreviewEmail() {

        var app = new OutlookApp();
        MailItem mailItem = (MailItem)app.CreateItem(OlItemType.olMailItem);
        mailItem.To = Recipients;
        mailItem.Subject = Subject;
        mailItem.Body = Body;

        Attachments.Where(File.Exists).ForEach(att => mailItem.Attachments.Add(att));

        var senderMailBox = EmailService.GetSender();
        var sender = GetSenderOutlookAccount(app, senderMailBox.Address);

        if (sender is not null) {
            mailItem.SendUsingAccount = sender;
        }

        mailItem.Display();

        Marshal.ReleaseComObject(app);
        Marshal.ReleaseComObject(mailItem);
        if (sender is not null) Marshal.ReleaseComObject(sender);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();

    }

    private static Account? GetSenderOutlookAccount(OutlookApp app, string preferredEmail) {

        var accounts = app.Session.Accounts;
        if (accounts is null || accounts.Count == 0) {
            return null;
        }

        Account? sender = null;
        foreach (Account account in accounts) {
            sender ??= account;
            if (account.SmtpAddress == preferredEmail) {
                sender = account;
                break;
            }
        }

        return sender;

    }

}
