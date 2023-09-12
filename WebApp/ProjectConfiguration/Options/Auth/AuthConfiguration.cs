namespace WebApp.ProjectConfiguration.Options.Auth
{
	public class AuthConfiguration
	{
		public const string FieldName = "AuthConfiguration";

		public List<string> PreaddedRoles { get; set; } = new List<string>();
		public List<ConfiguredUser> PreaddedUsers { get; set; } = new List<ConfiguredUser>();
	}
}
