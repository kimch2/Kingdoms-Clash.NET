﻿using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ClashEngine.NET.Graphics.Components
{
	using EntitiesManager;
	using Interfaces.Graphics.Components;

	/// <summary>
	/// Implemnetacja kamery ortogonalnej.
	/// </summary>
	public class OrthoCamera
		: RenderableComponent, IOrthoCamera
	{
		#region Private Fields
		/// <summary>
		/// Czy potrzeba uaktualnić.
		/// </summary>
		private bool NeedUpdate = false;
		#endregion

		#region IOrthoCamera Members
		#region Properties
		/// <summary>
		/// Granice kamery.
		/// </summary>
		public RectangleF Borders { get; private set; }

		/// <summary>
		/// Rozmiar kamery.
		/// </summary>
		public Vector2 Size { get; private set; }

		/// <summary>
		/// Aktualna pozycja(lewy górny róg).
		/// </summary>
		public Vector2 CurrentPosition { get; private set; }

		/// <summary>
		/// Szybkość poruszania kamery.
		/// </summary>
		public float CameraSpeed { get; private set; }

		/// <summary>
		/// Bliższa płaszczyzna kamery. Odpowiada parametrowi zNear GL.Ortho.
		/// </summary>
		public float ZNear { get; private set; }

		/// <summary>
		/// Dalsza płaszczyzna kamery. Odpowiada parametrowi zFar GL.Ortho.
		/// </summary>
		public float ZFar { get; private set; }

		/// <summary>
		/// Czy zawsze(co aktualizację komponentu, nie co przesunięcie) aktualizować macierz projekcji?
		/// </summary>
		public bool UpdateAlways { get; private set; }
		#endregion

		#region Methods
		/// <summary>
		/// Przesuwa kamerę na wskazaną pozycję.
		/// Jeśli pozycja jest poza zakresem automatycznie ją koryguje.
		/// </summary>
		/// <param name="pt">Lewy górny róg ekranu.</param>
		public void MoveTo(Vector2 pt)
		{
			if (pt.X < this.Borders.Left)
			{
				pt.X = this.Borders.Left;
			}
			else if (pt.X + this.Size.X > this.Borders.Right)
			{
				pt.X = this.Borders.Right - this.Size.X;
			}

			if (pt.Y < this.Borders.Top)
			{
				pt.Y = this.Borders.Top;
			}
			else if (pt.Y + this.Size.Y > this.Borders.Bottom)
			{
				pt.Y = this.Borders.Bottom - this.Size.Y;
			}

			//Uaktualniamy tylko jeśli pozycja się różni.
			if (this.CurrentPosition != pt || this.UpdateAlways)
			{
				this.CurrentPosition = pt;
				this.NeedUpdate = true;
			}
		}

		/// <summary>
		/// Inicjalizuje kamerę.
		/// Domyślnie ustawiana jest w lewym górnym rogu granic.
		/// </summary>
		/// <param name="borders">Krawędzie kamery.
		/// Width nie może być większe od size.Width i
		/// Height nie może być większe od size.Height.
		/// </param>
		/// <param name="size">Rozmiar.</param>
		/// <param name="speed">Szybkość poruszania się kamery.</param>
		/// <param name="updateAlways">Czy zawsze uaktualniać macierz projekcji?</param>
		/// <param name="zNear"><see cref="OrthoCamera.ZNear"/></param>
		/// <param name="zFar"><see cref="OrthoCamera.ZFar"/></param>
		public void Init(RectangleF borders, Vector2 size, float speed, bool updateAlways, float zNear = 0.0f, float zFar = 1.0f)
		{
			if (size.X > borders.Width || size.Y > borders.Height)
			{
				throw new ArgumentException("Size is greater than borders", "size");
			}
			this.Borders = borders;
			this.Size = size;
			this.CameraSpeed = speed;
			this.ZNear = zNear;
			this.ZFar = zFar;
			this.UpdateAlways = updateAlways;
			this.CurrentPosition = new Vector2(borders.Left, borders.Top);
		}
		#endregion
		#endregion

		#region Component Members
		/// <summary>
		/// Aktualizuje położenie kamery jeśli któryś z przycisków jest wciśnięty.
		/// </summary>
		/// <param name="delta"></param>
		public override void Update(double delta)
		{
			Vector2 pt = this.CurrentPosition;
			if (this.Input[OpenTK.Input.Key.Left])
			{
				pt.X -= (float)(delta * this.CameraSpeed);
			}
			if (this.Input[OpenTK.Input.Key.Right])
			{
				pt.X += (float)(delta * this.CameraSpeed);
			}
			if (this.Input[OpenTK.Input.Key.Up])
			{
				pt.Y -= (float)(delta * this.CameraSpeed);
			}
			if (this.Input[OpenTK.Input.Key.Down])
			{
				pt.Y += (float)(delta * this.CameraSpeed);
			}
			this.MoveTo(pt);
		}

		/// <summary>
		/// Uaktualnia macierze.
		/// </summary>
		public override void Render()
		{
			if (this.NeedUpdate || this.UpdateAlways)
			{
				this.UpdateMatrix();
				this.NeedUpdate = false;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Inicjalizuje kamerę.
		/// Domyślnie ustawiana jest w lewym górnym rogu granic.
		/// </summary>
		/// <param name="borders">Krawędzie kamery.
		/// Width nie może być większe od size.Width i
		/// Height nie może być większe od size.Height.
		/// </param>
		/// <param name="size">Rozmiar.</param>
		/// <param name="speed">Szybkość poruszania się kamery.</param>
		/// <param name="updateAlways">Czy zawsze uaktualniać macierz projekcji?</param>
		/// <param name="zNear"><see cref="OrthoCamera.ZNear"/></param>
		/// <param name="zFar"><see cref="OrthoCamera.ZFar"/></param>
		/// <exception cref="ArgumentException">Rozmiar jest większy od granic ekranu.</exception>
		public OrthoCamera(RectangleF borders, Vector2 size, float speed, bool updateAlways, float zNear = 0.0f, float zFar = 1.0f)
			: base("OrthoCamera")
		{
			this.NeedUpdate = true;
			this.Init(borders, size, speed, updateAlways, zNear, zFar);
		}
		#endregion

		#region Private Members
		/// <summary>
		/// Uaktualnia macież projekcji.
		/// </summary>
		private void UpdateMatrix()
		{
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(this.CurrentPosition.X, this.CurrentPosition.X + this.Size.X, this.CurrentPosition.Y + this.Size.Y, this.CurrentPosition.Y, this.ZNear, this.ZFar);

			GL.MatrixMode(MatrixMode.Modelview);
		}
		#endregion
	}
}