﻿using System;

namespace ClashEngine.NET.Interfaces.EntitiesManager
{
	/// <summary>
	/// Bazowy interfejs dla atrybutów encji.
	/// </summary>
	public interface IAttribute
		: IEquatable<IAttribute>
	{
		/// <summary>
		/// Identyfikator atrybutu.
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Wartość atrybutu.
		/// </summary>
		object Value { get; set; }
	}
}