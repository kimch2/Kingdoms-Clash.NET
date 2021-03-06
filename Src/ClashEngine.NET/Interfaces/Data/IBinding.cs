﻿using System;

namespace ClashEngine.NET.Interfaces.Data
{
	/// <summary>
	/// Tryb wiązania.
	/// </summary>
	public enum BindingMode
	{
		/// <summary>
		/// W jedną stronę - od źródła do bindowanej właściwości.
		/// </summary>
		OneWay,

		/// <summary>
		/// W obie strony.
		/// </summary>
		TwoWay,

		/// <summary>
		/// Pobiera wartość tylko raz, przy bindowaniu.
		/// </summary>
		OneTime
	}

	/// <summary>
	/// Wiąże wartości.
	/// </summary>
	public interface IBinding
		: IDisposable
	{
		/// <summary>
		/// Tryb bindowania.
		/// </summary>
		BindingMode Mode { get; }

		/// <summary>
		/// Obiekt źródłowy.
		/// </summary>
		object Source { get; }

		/// <summary>
		/// Źródłowa właściwość, z której będą pobierane wartości.
		/// </summary>
		IPropertyPath SourcePath { get; }

		/// <summary>
		/// Obiekt docelowy.
		/// </summary>
		object Target { get; }

		/// <summary>
		/// Docelowa właściwość, z której będą pobierane wartości.
		/// </summary>
		IPropertyPath TargetPath { get; }

		/// <summary>
		/// Typ konwertera użytego do konwersji pomiędzy końcami wiązania.
		/// </summary>
		Type ConverterType { get; }
	}
}
