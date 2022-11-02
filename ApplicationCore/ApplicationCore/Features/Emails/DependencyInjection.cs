using ApplicationCore.Features.Emails.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Emails;

internal static class DependencyInjection {

    public static IServiceCollection AddEmailing(this IServiceCollection services) {

        services.AddTransient<ITemplatedEmailService, TemplatedEmailService>();
        services.AddTransient<IEmailService, BasicEmailService>();
        services.AddTransient<ISmtpClient, MailKitSmtpClient>();
        services.AddTransient<ISmtpClientFactory, MailKitSmtpClientFactory>();

        return services;

    }

}
