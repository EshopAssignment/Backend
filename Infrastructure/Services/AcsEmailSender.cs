
using Application.Interfaces;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AcsEmailSender : IEmailSender
{
    private readonly EmailClient _client;
    private readonly string _from;
    private readonly ILogger<AcsEmailSender> _logger;


    public AcsEmailSender(IConfiguration config, Logger<AcsEmailSender> logger)
    {
        var cs = config["AcsEmail:ConnectionString"]
            ?? throw new InvalidOperationException("Missing ACS email connection string");

        _from = config["AcsEmail:From"]
            ?? throw new InvalidOperationException("Missing ACS email from address");

        _client = new EmailClient(cs);
        _logger = logger;
    }
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct)
    {
        var content = new EmailContent(subject)
        {
            Html = htmlBody
        };

        var message = new EmailMessage(_from, to, content);

        try
        {
            await _client.SendAsync(Azure.WaitUntil.Completed, message, ct);
            _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email sending failed to {Recipient}", to);
            throw;
        }

    }
}
