using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Logging;
using Rezqa.Infrastructure.Settings;
using Rezqa.Application.Interfaces;

namespace Rezqa.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(
        EmailSettings emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(string email, string userName, string token)
    {
        var verificationLink = $"{_emailSettings.BaseUrl}/identity/verify-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        var subject = "Verify your email address";
        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Verify Your Email</title>
            </head>
            <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
                <table role='presentation' style='width: 100%; border-collapse: collapse;'>
                    <tr>
                        <td style='padding: 20px 0; text-align: center; background-color: #ffffff;'>
                            <table role='presentation' style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                                <tr>
                                    <td style='padding: 40px 30px; text-align: center; background-color: #4a90e2; border-radius: 8px 8px 0 0;'>
                                        <h1 style='margin: 0; color: #ffffff; font-size: 24px;'>Welcome to Rezqa!</h1>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 40px 30px;'>
                                        <h2 style='margin: 0 0 20px; color: #333333; font-size: 20px;'>Hi {userName},</h2>
                                        <p style='margin: 0 0 20px; color: #666666; font-size: 16px; line-height: 1.5;'>
                                            Thank you for registering with Rezqa. To complete your registration and start using our services, 
                                            please verify your email address by clicking the button below:
                                        </p>
                                        <div style='text-align: center; margin: 30px 0;'>
                                            <a href='{verificationLink}' 
                                               style='background-color: #4a90e2; color: #ffffff; padding: 12px 30px; text-decoration: none; 
                                                      border-radius: 4px; font-weight: bold; display: inline-block;'>
                                                Verify Email Address
                                            </a>
                                        </div>
                                        <p style='margin: 0 0 20px; color: #666666; font-size: 14px; line-height: 1.5;'>
                                            If you did not create an account, you can safely ignore this email.
                                        </p>
                                        <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;'>
                                        <p style='margin: 0; color: #999999; font-size: 12px;'>
                                            Best regards,<br>
                                            <strong>The Rezqa Team</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetAsync(string email, string userName, string token)
    {
        var resetLink = $"{_emailSettings.BaseUrl}/identity/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        var subject = "Reset your password";
        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Reset Your Password</title>
            </head>
            <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
                <table role='presentation' style='width: 100%; border-collapse: collapse;'>
                    <tr>
                        <td style='padding: 20px 0; text-align: center; background-color: #ffffff;'>
                            <table role='presentation' style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                                <tr>
                                    <td style='padding: 40px 30px; text-align: center; background-color: #e74c3c; border-radius: 8px 8px 0 0;'>
                                        <h1 style='margin: 0; color: #ffffff; font-size: 24px;'>Password Reset Request</h1>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 40px 30px;'>
                                        <h2 style='margin: 0 0 20px; color: #333333; font-size: 20px;'>Hi {userName},</h2>
                                        <p style='margin: 0 0 20px; color: #666666; font-size: 16px; line-height: 1.5;'>
                                            We received a request to reset your password. To proceed with the password reset, 
                                            please click the button below:
                                        </p>
                                        <div style='text-align: center; margin: 30px 0;'>
                                            <a href='{resetLink}' 
                                               style='background-color: #e74c3c; color: #ffffff; padding: 12px 30px; text-decoration: none; 
                                                      border-radius: 4px; font-weight: bold; display: inline-block;'>
                                                Reset Password
                                            </a>
                                        </div>
                                        <p style='margin: 0 0 20px; color: #666666; font-size: 14px; line-height: 1.5;'>
                                            This link will expire in 1 hour for security reasons.<br>
                                            If you did not request a password reset, you can safely ignore this email.
                                        </p>
                                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin: 20px 0;'>
                                            <p style='margin: 0; color: #666666; font-size: 14px;'>
                                                <strong>Security Tip:</strong> Never share your password with anyone. 
                                                Our team will never ask for your password.
                                            </p>
                                        </div>
                                        <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;'>
                                        <p style='margin: 0; color: #999999; font-size: 12px;'>
                                            Best regards,<br>
                                            <strong>The Rezqa Team</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            _logger.LogInformation("Attempting to send email to {Email} using SMTP server {Server}:{Port}",
                to, _emailSettings.SmtpServer, _emailSettings.SmtpPort);

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };
            message.To.Add(to);

            _logger.LogInformation("Sending email from {FromEmail} to {ToEmail}",
                _emailSettings.FromEmail, to);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}. Error: {ErrorMessage}",
                to, ex.Message);
            throw new ApplicationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}