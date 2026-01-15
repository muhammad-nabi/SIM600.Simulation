using Azure;
using Azure.Communication.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace SIM600.Simulation.Services;

public class AzureEmailSender : IEmailSender
{
    private readonly AzureEmailOptions _options;
    private readonly ILogger<AzureEmailSender> _logger;
    private readonly EmailClient? _emailClient;

    public AzureEmailSender(
        IOptions<AzureEmailOptions> options,
        ILogger<AzureEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            try
            {
                _emailClient = new EmailClient(_options.ConnectionString);
                _logger.LogInformation("Azure Email Client initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure Email Client");
                _emailClient = null;
            }
        }
        else
        {
            _logger.LogWarning("Azure Email connection string not configured. Email sending is disabled.");
        }
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (_emailClient == null)
        {
            _logger.LogWarning(
                "Email not sent to {Email} (Subject: {Subject}). Email client not configured.",
                email, subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.SenderAddress))
        {
            _logger.LogError("Sender address not configured. Cannot send email.");
            return;
        }

        try
        {
            _logger.LogInformation(
                "Sending email to {Recipient} with subject: {Subject}",
                email, subject);

            var emailContent = new EmailContent(subject)
            {
                Html = htmlMessage
            };

            var emailMessage = new EmailMessage(
                senderAddress: _options.SenderAddress,
                recipientAddress: email,
                content: emailContent);

            EmailSendOperation emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                emailMessage);

            _logger.LogInformation(
                "Email sent successfully to {Recipient}. Operation ID: {OperationId}, Status: {Status}",
                email,
                emailSendOperation.Id,
                emailSendOperation.Value.Status);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex,
                "Azure Communication Services error sending email to {Recipient}. Status: {Status}, ErrorCode: {ErrorCode}",
                email,
                ex.Status,
                ex.ErrorCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email to {Recipient}", email);
            throw;
        }
    }
}
