﻿using System.Drawing;

namespace ClashEngine.NET.Cameras
{
	using EntitiesManager;
	using Interfaces.Cameras;

	/// <summary>
	/// Kamera ortograficzna 2D jako encja.
	/// <see cref="Interfaces.Components.Cameras.InnerCamera"/>
	/// </summary>
	public class OrthoCamera
		: GameEntity, IOrthoCamera
	{
		#region IOrthoCamera Members
		/// <summary>
		/// Kamera.
		/// </summary>
		public Interfaces.Components.Cameras.IOrthoCamera Camera { get; private set; }
		#endregion

		/// <summary>
		/// Inicjalizuje kamerę.
		/// Zobacz: <see cref="Interfaces.Components.Cameras.IOrthoCamera"/>
		/// </summary>
		public OrthoCamera(RectangleF borders, SizeF size, float speed, bool updateAlways, float zNear = 0.0f, float zFar = 1.0f)
			: base("OrthoCamera")
		{
			this.Camera = new Components.Cameras.OrthoCamera(borders, size, speed, updateAlways, zNear, zFar);
		}

		public override void InitEntity()
		{
			this.AddComponent(this.Camera);
		}
	}
}