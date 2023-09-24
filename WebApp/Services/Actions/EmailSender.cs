using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using WebApp.ProjectConfiguration.Options.Email;

namespace WebApp.Services.Actions
{
	public class EmailSender
	{
		private readonly SMTPCredentialsOptions _credentials;
		private readonly EmailConnectionInformationOptions _connection;

		public EmailSender(
			IOptions<SMTPCredentialsOptions> credentials,
			IOptions<EmailConnectionInformationOptions> connection)
		{
			_credentials = credentials.Value;
			_connection = connection.Value;
		}

		public MimeMessage GetMessage()
		{
			MimeMessage message = new();
			message.From.Add(new MailboxAddress(_credentials.EmailName, _credentials.Email));

			return message;
		}
		public async Task SendMessage(MimeMessage message)
		{
			using var sender = new SmtpClient();
			await sender.ConnectAsync(
				_connection.Host,
				_connection.Port,
				_connection.UseSsl
			);

			await sender.AuthenticateAsync(_credentials.Email, _credentials.Password);
			await sender.SendAsync(message);

			await sender.DisconnectAsync(true);
		}
	}
}
