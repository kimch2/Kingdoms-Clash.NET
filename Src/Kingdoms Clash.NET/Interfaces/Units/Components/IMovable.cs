﻿namespace Kingdoms_Clash.NET.Interfaces.Units.Components
{
	/// <summary>
	/// Komponent jednostki określający, że jednostka potrafi się poruszać.
	/// </summary>
	public interface IMovable
		: IUnitComponent
	{
		/// <summary>
		/// Prędkość jednostki.
		/// </summary>
		OpenTK.Vector2 Velocity { get; }
	}
}