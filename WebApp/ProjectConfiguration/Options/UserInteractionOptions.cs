namespace WebApp.ProjectConfiguration.Options
{
	public class UserInteractionOptions
	{
		public const string FieldName = "UserInteractionsStorageSettings";
		public int PerformCleanUpPeriodHours { get; set; }

		public int StoreViewInformationDays { get; set; }
		public int StoreRatedInformationDays { get; set; }
	}
}
