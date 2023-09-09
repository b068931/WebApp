using System.Text;
using WebApp.Database;
using WebApp.Database.Entities.Grouping;

namespace WebApp.Utilities.Other
{
	public class CategoriesDrawer
	{
		private readonly int _specifiedDepth;
		private readonly string _paddingCharacters;
		private readonly char _entryCharacter;
		private readonly string _newLine;
		private readonly DatabaseContext _database;
		public CategoriesDrawer(int specifiedDepth, string paddingCharacters, char entryCharacter, string newLine, DatabaseContext database)
		{
			_specifiedDepth = specifiedDepth;
			_paddingCharacters = paddingCharacters;
			_entryCharacter = entryCharacter;
			_newLine = newLine;
			_database = database;
		}

		private void AddLines(StringBuilder builder, int count)
		{
			builder.Insert(builder.Length, _paddingCharacters, count);
			builder.Append(_entryCharacter);
		}

		private void RecursiveDrawCategories(StringBuilder builder, Category category, int currentDepth)
		{
			if (currentDepth == 0) return;
			AddLines(builder, _specifiedDepth - currentDepth);

			builder.Append(
				string.Format(
					"<span class='category' data-myid='{0}' role='button'{2}>{1}</span>",
					category.Id,
					category.Name,
					category.IsPopular ? " style='color:#d68100'" : ""
				)
			);

			builder.Append(_newLine);

			_database.Categories.Entry(category).Collection(e => e.Children).Load();
			foreach (Category childCategory in category.Children)
			{
				RecursiveDrawCategories(builder, childCategory, currentDepth - 1);
			}
		}

		public string DrawCategories(Category category)
		{
			StringBuilder builder = new StringBuilder("<pre>");
			RecursiveDrawCategories(builder, category, _specifiedDepth);

			builder.Append("</pre>");
			return builder.ToString();
		}
	}
}
