using WebApp.Database.Models;

namespace WebApp.Services.Interfaces.Products
{
	public interface IColoursManager
	{
		List<Colour> GetAllColours();

		void CreateColour(string name, string hexCode);
		void UpdateColour(int id, string name, string hexCode);
		void DeleteColour(int id);
	}
}
