﻿using System;
using System.Drawing;

namespace ClashEngine.NET.Graphics.Resources
{
	using Interfaces.Graphics.Resources;

	/// <summary>
	/// Czcionka systemowa.
	/// Używa System.Drawing.Font.
	/// </summary>
	public class SystemFont
		: IFont
	{
		private static NLog.Logger Logger = NLog.LogManager.GetLogger("ClashEngine.NET");

		#region Private fields
		private Bitmap MeasuringBitmap;
		private System.Drawing.Graphics Measuring;
		private Font Font;
		#endregion
		
		#region IFont Members
		/// <summary>
		/// Nazwa czcionki.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Rozmiar w pikselach.
		/// </summary>
		public int Size { get; private set; }

		/// <summary>
		/// Czy jest to czcionka pogrubiona.
		/// </summary>
		public bool Bold { get; private set; }

		/// <summary>
		/// Czy jest to czcionka pochylona.
		/// </summary>
		public bool Italic { get; private set; }

		/// <summary>
		/// Rysuje tekst do nowej tekstury.
		/// Nowo utworzona tekstura ma Id równe "Text.(text)" i jest dodana do managera czcionki.
		/// </summary>
		/// <param name="text">Tekst.</param>
		/// <param name="color">Kolor.</param>
		/// <exception cref="ArgumentNullException">onto jest nullem lub text jest pusty/nullem.</exception>
		/// <returns>Nowo utworzona tekstura.</returns>
		public ITexture DrawString(string text, Color color)
		{
			ITexture tex = this.CreateEmptyText();
			this.DrawString(text, color, tex);
			return tex;
		}

		/// <summary>
		/// Rysuje tekst na istniejącą teksturę.
		/// Jeśli onto ma puste Id to ustawia mu je na wartość "Text.(text)" i rejestruje w managerze.
		/// </summary>
		/// <param name="text">Tekst do wypiania.</param>
		/// <param name="color">Kolor.</param>
		/// <param name="onto">Tekstura. Musi być to tekstura zwrócona przez DrawString.</param>
		/// <exception cref="ArgumentNullException">onto jest nullem lub text jest pusty/nullem.</exception>
		/// <exception cref="ArgumentException">Tekstura onto nie była stworzona przez metodę CreateEmptyText.</exception>
		public void DrawString(string text, Color color, ITexture onto)
		{
			if (onto == null)
			{
				throw new ArgumentNullException("onto");
			}
			else if (!(onto is Internals.ChangableTexture))
			{
				throw new ArgumentException("Texture wasn't created by CreateEmptyText method", "onto");
			}
			else if (string.IsNullOrEmpty(onto.Id))
			{
				onto.Id = "Text." + text;
				this.Manager.Add(onto);
			}

			var size = this.Measuring.MeasureString(text, this.Font);
			using (var bm = new Bitmap((int)size.Width, (int)size.Height))
			{
				using (var g = System.Drawing.Graphics.FromImage(bm))
				{
					g.FillRectangle(Brushes.Transparent, 0, 0, bm.Width, bm.Height);
					g.DrawString(text, this.Font, new SolidBrush(color), 0f, 0f);
				}
				(onto as Internals.ChangableTexture).Set(bm);
			}
		}

		/// <summary>
		/// Tworzy pustą teksturę, by móc jej później użyć w metodzie <see cref="DrawString(text,ITexture)"/>.
		/// </summary>
		/// <returns>Nowa tekstura.</returns>
		public ITexture CreateEmptyText()
		{
			return new Internals.ChangableTexture();
		}
		#endregion

		#region IResource Members
		/// <summary>
		/// Identyfikator czcionki - nazwa,rozmiar_w_pikselach[,ib]
		/// i - pochyła
		/// b - pogrubiona
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Nazwa czcionki - identycznie jak <see cref="Id"/>.
		/// </summary>
		public string FileName
		{
			get { return this.Id; }
			set { }
		}

		/// <summary>
		/// Manager-rodzic zasobu.
		/// </summary>
		public Interfaces.IResourcesManager Manager { get; set; }

		/// <summary>
		/// Wczytuje czcionkę parsując Id.
		/// </summary>
		/// <returns>Failure, gdy Id ma niepoprawny format, DefaultUsed, gdy nie znaleziono czcionki, inaczej Success.</returns>
		public Interfaces.ResourceLoadingState Load()
		{
			string[] values = this.Id.Split(',');
			int size = 0;
			if (values.Length != 3 || !int.TryParse(values[1].Trim(), out size))
			{
				return Interfaces.ResourceLoadingState.Failure;
			}
			this.Name = values[0].Trim();
			this.Size = size;
			this.Italic = values[2].Contains("i");
			this.Bold = values[2].Contains("b");

			this.Font = new Font(this.Name, this.Size,
				(this.Italic ? FontStyle.Italic : FontStyle.Regular) | (this.Bold ? FontStyle.Bold : FontStyle.Regular), GraphicsUnit.Pixel);
			if (this.Font.Name == "Microsoft Sans Serif")
			{
				return Interfaces.ResourceLoadingState.DefaultUsed;
			}

			return Interfaces.ResourceLoadingState.Success;
		}

		/// <summary>
		/// Zwalnia czcionkę.
		/// </summary>
		public void Free()
		{
			this.Measuring.Dispose();
			this.MeasuringBitmap.Dispose();
			this.Font.Dispose();
		}
		#endregion

		public SystemFont()
		{
			this.MeasuringBitmap = new Bitmap(1, 1);
			this.Measuring = System.Drawing.Graphics.FromImage(this.MeasuringBitmap);
		}

		#region IDisposable Members
		public void Dispose()
		{
			this.Free();
		}
		#endregion
	}
}