﻿using ClashEngine.NET.Interfaces.EntitiesManager;

namespace Kingdoms_Clash.NET.Interfaces.Map
{
	using Resources;

	/// <summary>
	/// Mapa.
	/// Implementacja powinna zadbać o odnawianie się zasobów(jeśli są odnawialne).
	/// </summary>
	public interface IMap
		: IGameEntity
	{
		/// <summary>
		/// Nazwa mapy.
		/// </summary>
		string Name { get; }
		
		/// <summary>
		/// Rozmiar mapy.
		/// Musi być stały dla danej mapy.
		/// </summary>
		/// <remarks>
		/// Jedna "jednostka" gry odpowiada jednemu "ekranowi"(czyli szerokości okna w pikselach).
		/// </remarks>
		float Size { get; }

		/// <summary>
		/// Sprawdza czy pomiędzy dwoma punktami jest jakiś zasób.
		/// </summary>
		/// <param name="beginig">Początek.</param>
		/// <param name="end">Koniec szukania.</param>
		/// <param name="position">Pozycja na której odnaleziono zasób.</param>
		/// <returns>Pakunek z zasobem lub null, gdy nie znaleziono.</returns>
		IResource CheckForResource(float beginig, float end, out float position);

		/// <summary>
		/// Resetuje stan gry(zaczyna ją od nowa).
		/// Po resecie mapa powinna być tak samo dziewicza jak przed.
		/// </summary>
		void Reset();
	}
}