﻿using System.Drawing;
using OpenTK;

namespace ClashEngine.NET.Graphics.Cameras
{
	using Interfaces.Graphics.Cameras;

	/// <summary>
	/// Kamera 2D, którą można poruszać.
	/// </summary>
	public class Movable2DCamera
		: IMovable2DCamera
	{
		#region Private fields
		private Vector2 _CurrentPosition = Vector2.Zero;
		private Matrix4 _ViewMatrix = Matrix4.Identity;
		private Matrix4 _ProjectionMatrix = Matrix4.Identity;
		private RectangleF _Borders = RectangleF.Empty;
		private Vector2 _Size = Vector2.Zero;
		#endregion

		#region IMovable2DCamera Members
		/// <summary>
		/// Zakres widoczności kamery.
		/// </summary>	
		public RectangleF Borders
		{
			get { return this._Borders; }
			set
			{
				if (this.Size.X > value.Width ||
					this.Size.Y > value.Height)
				{
					throw new System.ArgumentException("Camera size must be less than or equal to borders reactangle size", "value");
				}
				this.CurrentPosition = this.CurrentPosition;
				this._Borders = value;
			}
		}

		/// <summary>
		/// Aktualna pozycja kamery.
		/// </summary>
		public Vector2 CurrentPosition
		{
			get { return this._CurrentPosition; }
			set
			{
				this._CurrentPosition = value;

				//Korygujemy ewentualne wyjście poza granice
				if (this.CurrentPosition.X < this.Borders.Left)
				{
					this._CurrentPosition.X = this.Borders.Left;
				}
				if (this.CurrentPosition.Y < this.Borders.Top)
				{
					this._CurrentPosition.Y = this.Borders.Top;
				}
				if (this.CurrentPosition.X + this.Size.X > this.Borders.Right)
				{
					this._CurrentPosition.X = this.Borders.Right - this.Size.X;
				}
				if (this.CurrentPosition.Y + this.Size.Y > this.Borders.Bottom)
				{
					this._CurrentPosition.Y = this.Borders.Bottom - this.Size.Y;
				}


				this.NeedUpdate = true;
			}
		}

		/// <summary>
		/// Zmienia(równolegle) rozmiar i zakres widoczności kamery.
		/// </summary>
		/// <param name="size">Rozmiar.</param>
		/// <param name="borders">Zakres widoczności.</param>
		public void Change(Vector2 size, RectangleF borders)
		{
			if (size.X > borders.Width ||
				size.Y > borders.Height)
			{
				throw new System.ArgumentException("Camera size must be less than or equal to borders reactangle size", "size");
			}
			this._Size = size;
			this._Borders = borders;
			this.NeedUpdate = true;
			this.CurrentPosition = this.CurrentPosition;
		}
		#endregion

		#region ICamera Members
		/// <summary>
		/// Rozmiar kamery.
		/// </summary>
		public OpenTK.Vector2 Size {
			get { return this._Size; }
			set
			{
				if (value.X > this.Borders.Width ||
					value.Y > this.Borders.Height)
				{
					throw new System.ArgumentException("Camera size must be less than or equal to borders reactangle size", "value");
				}
				this._Size = value;
				this.NeedUpdate = true;
			}
		}

		/// <summary>
		/// Zawsze 0.
		/// </summary>
		public float ZNear { get { return 0f; } }

		/// <summary>
		/// Zawsze 1.
		/// </summary>
		public float ZFar { get { return 1f; } }

		/// <summary>
		/// Czy jest potrzebna aktualizacja.
		/// </summary>
		public bool NeedUpdate { get; set; }

		/// <summary>
		/// Macierz widoku.
		/// </summary>
		public OpenTK.Matrix4 ViewMatrix { get { return this._ViewMatrix; } }

		/// <summary>
		/// Macierz projektcji.
		/// Jest uaktualniana tylko wtedy, kiedy potrzeba.
		/// </summary>
		public OpenTK.Matrix4 ProjectionMatrix
		{
			get
			{
				if (this.NeedUpdate)
				{
					Matrix4.CreateOrthographicOffCenter(
						this.CurrentPosition.X, this.CurrentPosition.X + this.Size.X,
						this.CurrentPosition.Y + this.Size.Y, this.CurrentPosition.Y,
						this.ZNear, this.ZFar, out this._ProjectionMatrix);
				}
				return this._ProjectionMatrix;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Inicjalizuje kamerę.
		/// </summary>
		/// <param name="size">Rozmiar.</param>
		/// <param name="borders">Granice.</param>
		public Movable2DCamera(Vector2 size, RectangleF borders)
		{
			this._Size = size;
			this._Borders = borders;
			this.NeedUpdate = true;

			if (this.Size.X > this.Borders.Width ||
				this.Size.Y > this.Borders.Height)
			{
				throw new System.ArgumentException("Camera size must be less than or equal to borders reactangle size.", "size");
			}
		}
		#endregion

		#region Entity
		/// <summary>
		/// Tworzy encję gry, która automatycznie będzie zmieniać pozycję tej kamery.
		/// </summary>
		/// <param name="cameraSpeed">Szybkość poruszania kamerą.</param>
		/// <returns></returns>
		public Interfaces.EntitiesManager.IGameEntity GetCameraEntity(float cameraSpeed)
		{
			return new Entity(this, cameraSpeed);
		}

		/// <summary>
		/// Prywatna klasa encji.
		/// </summary>
		private class Entity
			: EntitiesManager.GameEntity
		{
			private IMovable2DCamera ParentCamera;
			private float CameraSpeed;

			public Entity(IMovable2DCamera camera, float speed)
				: base("Movable2DCameraEntity")
			{
				this.ParentCamera = camera;
				this.CameraSpeed = speed;
			}

			public override void Update(double delta)
			{
				Vector2 pt = this.ParentCamera.CurrentPosition;
				if (this.GameInfo.MainWindow.Input[OpenTK.Input.Key.Left])
				{
					pt.X -= (float)(delta * this.CameraSpeed);
				}
				if (this.GameInfo.MainWindow.Input[OpenTK.Input.Key.Right])
				{
					pt.X += (float)(delta * this.CameraSpeed);
				}
				if (this.GameInfo.MainWindow.Input[OpenTK.Input.Key.Up])
				{
					pt.Y -= (float)(delta * this.CameraSpeed);
				}
				if (this.GameInfo.MainWindow.Input[OpenTK.Input.Key.Down])
				{
					pt.Y += (float)(delta * this.CameraSpeed);
				}
				this.ParentCamera.CurrentPosition = pt;
			}
		}
		#endregion
	}
}
