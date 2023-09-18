namespace WebApp.ProjectConfiguration.Options.Email
{
    public class SMTPCredentialsOptions
    {
        public const string FieldName = "EmailSMTPCredentials";

        public string EmailName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
