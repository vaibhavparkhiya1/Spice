using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Spice.Service
{
	public class EmailSender : IEmailSender
	{

		public EmailOptions Options { get; set; }

		public EmailSender(IOptions<EmailOptions> emailOptions)
		{
			Options = emailOptions.Value;
		}



		public Task SendEmailAsync(string email, string subject, string Message)
		{
			return Execute(Options.SendGridKey,subject,Message,email); 
		}

		private Task Execute(string sendGridKey, string subject, string message, string email)
		{
			var client = new SendGridClient(sendGridKey);
			var msg = new SendGridMessage()
			{
				From = new EmailAddress("admin@spice.com", "Spice Restaurant"),
				Subject = subject,
				PlainTextContent = message,
				HtmlContent = message
			};
			msg.AddTo(new EmailAddress(email));
			try
			{
				return client.SendEmailAsync(msg);
			}
			catch (Exception ex)
			{

			}
			return null;
		}
	}
}
