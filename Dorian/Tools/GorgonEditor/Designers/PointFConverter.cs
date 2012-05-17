#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Thursday, May 17, 2012 10:32:41 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.ComponentModel;
using System.Windows.Forms.Design;
using System.Reflection;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Class to extend the PointConverter class to handle floating point point values.
	/// </summary>
	public class PointFConverter
		: TypeConverter
	{
		#region Methods.
		/// <summary>
		/// Returns whether this converter can convert the object to the specified type, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
		/// <param name="destinationType">A <see cref="T:System.Type"></see> that represents the type you want to convert to.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
				return true;

			return base.CanConvertTo(context, destinationType);
		}

		/// <summary>
		/// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
		/// <param name="sourceType">A <see cref="T:System.Type"></see> that represents the type you want to convert from.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		/// Converts the given object to the type of this converter, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
		/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> to use as the current culture.</param>
		/// <param name="value">The <see cref="T:System.Object"></see> to convert.</param>
		/// <returns>
		/// An <see cref="T:System.Object"></see> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			string text = value as string;		// Value to convert.

			if ((text != null) && (text.Trim().Length != 0))
			{
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));		// Get the floating point type converter.
				PointF point = new PointF(0, 0);											// New point.

				// Clip off spaces.
				text = text.Trim();

				// Get the culture.
				if (culture == null)
					culture = System.Globalization.CultureInfo.CurrentCulture;

				// Get the separator for the value.
				char ch = culture.TextInfo.ListSeparator[0];

				// Get the individual values.
				string[] xy = text.Split(new char[] { ch });				
				
				if (xy.Length != 2)
					throw new ArgumentException("Unable to convert the value into a point format.  Number of values is incorrect.");

				// Convert to floating point values.
				point.X = (float)converter.ConvertFromString(context, culture, xy[0]);
				point.Y = (float)converter.ConvertFromString(context, culture, xy[1]);

				return point;
			}

			return base.ConvertFrom(context, culture, value);
		}

		/// <summary>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
		/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"></see>. If null is passed, the current culture is assumed.</param>
		/// <param name="value">The <see cref="T:System.Object"></see> to convert.</param>
		/// <param name="destinationType">The <see cref="T:System.Type"></see> to convert the value parameter to.</param>
		/// <returns>
		/// An <see cref="T:System.Object"></see> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		/// <exception cref="T:System.ArgumentNullException">The destinationType parameter is null. </exception>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				PointF ptValue = (PointF)value;		// Point value.

				return ptValue.X.ToString() + ", " + ptValue.Y.ToString();
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// Gets the properties supported.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context)
		{
			return true;
		}

		/// <summary>
		/// Returns a collection of properties for the type of array specified by the value parameter, using the specified context and attributes.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
		/// <param name="value">An <see cref="T:System.Object"></see> that specifies the type of array for which to get properties.</param>
		/// <param name="attributes">An array of type <see cref="T:System.Attribute"></see> that is used as a filter.</param>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"></see> with the properties that are exposed for this data type, or null if there are no properties.
		/// </returns>
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties(typeof(PointF), attributes).Sort(new string[] {"X", "Y"});
		}
		#endregion
	}
}
