﻿namespace ClashEngine.NET.Interfaces.Graphics.Cameras
{
	using Interfaces.EntitiesManager;

	/// <summary>
	/// Kamera ortograficzna 2D jako encja.
	/// <see cref="Interfaces.Components.Cameras.InnerCamera"/>
	/// </summary>
	public interface IOrthoCamera
		: IGameEntity
	{
		/// <summary>
		/// Kamera.
		/// </summary>
		Components.IOrthoCamera Camera { get; }
	}
}