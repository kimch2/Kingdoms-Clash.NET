﻿using System.Drawing;
using OpenTK;

namespace Kingdoms_Clash.NET
{
	/// <summary>
	/// Konfiguracja gry.
	/// </summary>
	public class Configuration
		: Interfaces.IConfiguration
	{
		private static NLog.Logger Logger = NLog.LogManager.GetLogger("KingdomsClash.NET");

		#region Singleton
		private static Interfaces.IConfiguration _Instance;

		/// <summary>
		/// Globalna instancja konfiguracji.
		/// </summary>
		public static Interfaces.IConfiguration Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Instance = new Configuration();
				}
				return _Instance;
			}
		}

		/// <summary>
		/// Wymusza użycie domyślnej konfiguracji.
		/// </summary>
		public static void UseDefault()
		{
			Logger.Info("Using default configuration");
			_Instance = Defaults.DefaultClientConfiguration;
		}
		#endregion

		#region IConfiguration Members
		/// <summary>
		/// Rozmiar okna.
		/// </summary>
		public Size WindowSize { get; set; }

		/// <summary>
		/// Czy okno ma być pełnoekranowe.
		/// </summary>
		public bool Fullscreen { get; set; }

		/// <summary>
		/// Czy używać synchronizacji pionowej.
		/// </summary>
		public bool VSync { get; set; }

		/// <summary>
		/// Szybkość poruszania się kamery.
		/// </summary>
		public float CameraSpeed { get; set; }

		/// <summary>
		/// Czy używać licznika FPS.
		/// </summary>
		public bool UseFPSCounter { get; set; }

		/// <summary>
		/// Nacja pierwszego gracza.
		/// </summary>
		public string Player1Nation { get; set; }

		/// <summary>
		/// Nacja drugiego gracza.
		/// </summary>
		public string Player2Nation { get; set; }
		#endregion

		internal Configuration()
		{ }

		public void Set(Interfaces.IConfiguration conf)
		{
			this.CameraSpeed = conf.CameraSpeed;
			this.Fullscreen = conf.Fullscreen;
			this.Player1Nation = conf.Player1Nation;
			this.Player2Nation = conf.Player2Nation;
			this.UseFPSCounter = conf.UseFPSCounter;
			this.VSync = conf.VSync;
			this.WindowSize = conf.WindowSize;
		}
	}
}
