namespace WebApp.ProjectConfiguration.Options.Email
{
	public class EmailConnectionInformationOptions
	{
		public const string FieldName = "EmailConnection";

		public string Host { get; set; } = default!;
		public int Port { get; set; }
		public bool UseSsl { get; set; }
	}
}
