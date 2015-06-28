#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, June 28, 2015 2:57:12 PM
// 
#endregion

using System;
using System.IO;
using System.Runtime.InteropServices;
using Gorgon.IO;

namespace Gorgon.Native
{
	/// <summary>
	/// Wraps unmanaged native memory pointed at by a pointer and provides safe access to that pointer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This wraps a pointer to unmanaged native memory that has been allocated, pinned or aliased. It provides better thread safety than the <see cref="Gorgon.IO.GorgonDataStream"/> object and gives safe access when  
	/// dereferencing the pointer to read or write data.
	/// </para>
	/// <para>
	/// There are several types that implement this interface, and each of them handle a pointer differently:
	/// <list type="table">
	///		<listheader>
	///			<term>Type</term>
	///			<description>Description</description>
	///		</listheader>
	///		<item>
	///			<term><see cref="GorgonPointer"/></term>
	///			<description>
	///			Allocates a block of unmanaged memory provides access to the pointer that points to the unmanaged block. This memory can be aligned and the memory allocated is owned by the object and must be freed 
	///			manually.
	///			</description> 
	///		</item>
	///		<item>
	///			<term><see cref="GorgonPointerTyped{T}"/></term>
	///			<description>
	///			Allocates a block of unmanaged memory large enough to fit the type specified by the generic type parameter and provides access to the pointer that points to the unmanaged block. This memory can be aligned 
	///			and the memory allocated is owned by the object and must be freed manually.
	///			</description> 
	///		</item>
	///		<item>
	///			<term><see cref="GorgonPointerPinned{T}"/></term>
	///			<description>
	///			Pins a managed value type specified by the generic type parameter to a fixed location and provides raw access to the data for that type. This memory is aligned by the .NET memory management system. 
	///			The handle returning the pointer to the memory is owned by the the object and must be freed manually.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="GorgonPointerAlias"/></term>
	///			<description>
	///			Aliases an existing unsafe <c>void *</c>, or <see cref="IntPtr"/> (for languages that don't support unsafe access) and provides access to the memory pointed at by the pointer passed in. The memory is 
	///			<b>not</b> owned by the object and will not be freed by this object.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="GorgonPointerAliasTyped{T}"/></term>
	///			<description>
	///			Aliases an existing unsafe <c>void *</c>, or <see cref="IntPtr"/> (for languages that don't support unsafe access) and provides access to the memory pointed at by the pointer passed in. The type provided 
	///			by the generic type parameter is used determine the size of the data for the type, in bytes. The memory is <b>not</b> owned by the object and will not be freed by this object.
	///			</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// Like the <see cref="Gorgon.IO.GorgonDataStream"/> object, it provides several methods to <see cref="O:Gorgon.Native.IGorgonPointer.Read">read</see>/<see cref="O:Gorgon.Native.IGorgonPointer.Write">write</see> 
	/// data to the memory pointed at by the internal pointer. With these methods the object can read and write primitive types, arrays, and generic value types.
	/// </para>
	/// <para>
	/// Please visit the documentation for each of the <c>GorgonPointer</c> types in the <b>see also</b> section for more detailed information.
	/// </para>
	/// </remarks>
	/// <seealso cref="Gorgon.IO.GorgonDataStream"/>
	/// <seealso cref="GorgonPointer"/>
	/// <seealso cref="GorgonPointerTyped{T}"/>
	/// <seealso cref="GorgonPointerPinned{T}"/>
	/// <seealso cref="GorgonPointerAlias"/>
	/// <seealso cref="GorgonPointerAliasTyped{T}"/>
	public interface IGorgonPointer 
		: IDisposable
	{
		/// <summary>
		/// Property to return whether the pointer has been disposed or not.
		/// </summary>
		/// <remarks>
		/// This property is used to determine if the memory has been released by the object.
		/// </remarks>
		bool Disposed
		{
			get;
		}

		/// <summary>
		/// Property to return the address of the pointer.
		/// </summary>
		long Address
		{
			get;
		}

		/// <summary>
		/// Property to return the size of the data pointed at by this pointer, in bytes.
		/// </summary>
		long Size
		{
			get;
		}

		/// <summary>
		/// Function to read a value from the unmanaged memory at the given offset.
		/// </summary>
		/// <typeparam name="T">Type of value to retrieve. Must be a value or primitive type.</typeparam>
		/// <param name="offset">The offset within unmanaged memory to start reading from, in bytes.</param>
		/// <param name="value">The value retrieved from unmanaged memory.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> value is less than 0.</exception>
		/// <remarks>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		void Read<T>(long offset, out T value)
			where T : struct;

		/// <summary>
		/// <inheritdoc cref="Read{T}(long,out T)"/>
		/// </summary>
		/// <typeparam name="T"><inheritdoc cref="Read{T}(long,out T)"/></typeparam>
		/// <param name="offset"><inheritdoc cref="Read{T}(long,out T)"/></param>
		/// <returns>The value from unmanaged memory.</returns>
		/// <exception cref="InvalidOperationException"><inheritdoc cref="Read{T}(long,out T)"/></exception>
		/// <exception cref="ArgumentOutOfRangeException"><inheritdoc cref="Read{T}(long,out T)"/></exception>
		/// <remarks>
		/// <inheritdoc cref="Read{T}(long,out T)"/>
		/// </remarks>
		T Read<T>(long offset)
			where T : struct;

		/// <summary>
		/// Function to read a value from the beginning of the unmanaged memory pointed at by this object.
		/// </summary>
		/// <typeparam name="T"><inheritdoc cref="Read{T}(long,out T)"/></typeparam>
		/// <param name="value"><inheritdoc cref="Read{T}(long,out T)"/></param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>Read&lt;T&gt;(0, out value)</c>, except that it does not perform any addition to the pointer with an offset. Thus making this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <inheritdoc cref="Read{T}(long,out T)"/>
		/// </remarks>
		void Read<T>(out T value)
			where T : struct;

		/// <summary>
		/// Function to read a value from the beginning of the unmanaged memory pointed at by this object.
		/// </summary>
		/// <typeparam name="T"><inheritdoc cref="Read{T}(long,out T)"/></typeparam>
		/// <returns><inheritdoc cref="Read{T}(long)"/></returns>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>T value = Read&lt;T&gt;(0)</c>, except that it does not perform any addition to the pointer with an offset. Thus making this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <inheritdoc cref="Read{T}(long,out T)"/>
		/// </remarks>
		T Read<T>()
			where T : struct;

		/// <summary>
		/// Function to write a value to unmanaged memory at the given offset.
		/// </summary>
		/// <typeparam name="T">Type of value to write. Must be a value or primitive type.</typeparam>
		/// <param name="offset">The offset within unmanaged memory to start writing into, in bytes.</param>
		/// <param name="value">The value to write to unmanaged memory.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> value is less than 0.</exception>
		/// <remarks>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to write, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		void Write<T>(long offset, ref T value)
			where T : struct;

		/// <inheritdoc cref="Write{T}(long,ref T)"/>
		void Write<T>(long offset, T value)
			where T : struct;

		/// <summary>
		/// Function to write a value into the beginning of the unmanaged memory pointed at by this object.
		/// </summary>
		/// <typeparam name="T"><inheritdoc cref="Write{T}(long,ref T)"/></typeparam>
		/// <param name="value"><inheritdoc cref="Write{T}(long,ref T)"/></param>
		/// <exception cref="InvalidOperationException"><inheritdoc cref="Write{T}(long,ref T)"/></exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>Write&lt;T&gt;(0, value)</c>, except that it does not perform any addition to the pointer with an offset. Thus making this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <inheritdoc cref="Write{T}(long,ref T)"/>
		/// </remarks>
		void Write<T>(ref T value)
			where T : struct;

		/// <inheritdoc cref="Write{T}(ref T)"/>
		void Write<T>(T value)
			where T : struct;

		/// <summary>
		/// Function to read a range of values from unmanaged memory, at the specified offset, into an array.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the buffer to start reading from, in bytes.</param>
		/// <param name="array">The array that will receive the data from unmanaged memory.</param>
		/// <param name="index">The index in the array to start at.</param>
		/// <param name="count">The number of items to fill the array with.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + the <paramref name="count"/> is larger than the total length of <paramref name="array"/>.
		/// <para>-or-</para>
		/// <para>Thrown when a buffer overrun is detected because the size, in bytes, of the <paramref name="count"/> plus the <paramref name="offset"/> is too large for the buffer.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than 0.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		void ReadRange<T>(long offset, T[] array, int index, int count)
			where T : struct;

		/// <summary>
		/// Function to read a range of values into an array from the beginning of unmanaged memory pointed at by this object.
		/// </summary>
		/// <typeparam name="T"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></typeparam>
		/// <param name="array"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></param>
		/// <param name="index"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></param>
		/// <param name="count"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></param>
		/// <exception cref="ArgumentNullException"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></exception>
		/// <exception cref="ArgumentOutOfRangeException"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></exception>
		/// <exception cref="ArgumentException"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>ReadRange&lt;T&gt;(0, array, index, count)</c>, except that it does not perform any addition to the pointer with an offset. Thus making this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <inheritdoc cref="ReadRange{T}(long,T[],int,int)"/>
		/// </remarks>
		void ReadRange<T>(T[] array, int index, int count)
			where T : struct;

		/// <summary>
		/// <inheritdoc cref="ReadRange{T}(long,T[],int,int)"/>
		/// </summary>
		/// <typeparam name="T"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></typeparam>
		/// <param name="offset"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></param>
		/// <param name="array"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></param>
		/// <exception cref="ArgumentNullException"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length plus the <paramref name="offset"/> is too large for the buffer.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than 0.</para>
		/// </exception>
		/// <remarks>
		/// <inheritdoc cref="ReadRange{T}(long,T[],int,int)"/>
		/// </remarks>
		void ReadRange<T>(long offset, T[] array)
			where T : struct;

		/// <summary>
		/// <inheritdoc cref="ReadRange{T}(T[],int,int)"/>
		/// </summary>
		/// <typeparam name="T"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></typeparam>
		/// <param name="array"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></param>
		/// <exception cref="ArgumentNullException"><inheritdoc cref="ReadRange{T}(long,T[],int,int)"/></exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length is too large for the buffer.</exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>ReadRange&lt;T&gt;(0, array, index, count)</c>, except that it does not perform any addition to the pointer with an offset. Thus making this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <inheritdoc cref="ReadRange{T}(long,T[],int,int)"/>
		/// </remarks>
		void ReadRange<T>(T[] array)
			where T : struct;

		/// <summary>
		/// Function to write a range of values from an array into this buffer.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the buffer to start writing, in bytes.</param>
		/// <param name="array">The array to copy into the buffer.</param>
		/// <param name="index">The index in the array to start copying from.</param>
		/// <param name="count">The number of items to in the array to copy into the buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + the <paramref name="count"/> is larger than the total length of <paramref name="array"/>.
		/// <para>-or-</para>
		/// <para>Thrown when a buffer overrun is detected because the size, in bytes, of the <paramref name="count"/> plus the <paramref name="offset"/> is too large for the buffer.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than 0.</para>
		/// </exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		void WriteRange<T>(long offset, T[] array, int index, int count)
			where T : struct;

		/// <summary>
		/// Function to write a range of values from an array into this buffer.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the buffer to start writing, in bytes.</param>
		/// <param name="array">The array to copy into the buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length plus the <paramref name="offset"/> is too large for the buffer.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than zero.</para>
		/// </exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		void WriteRange<T>(long offset, T[] array)
			where T : struct;

		/// <summary>
		/// Function to write a range of values from an array into the beginning of this buffer.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">The array to copy into the buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length is too large for the buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		void WriteRange<T>(T[] array)
			where T : struct;

		/// <summary>
		/// Function to write a range of values from an array into the beginning of this buffer.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">The array to copy into the buffer.</param>
		/// <param name="index">The index in the array to start copying from.</param>
		/// <param name="count">The number of items to in the array to copy into the buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + the <paramref name="count"/> is larger than the total length of <paramref name="array"/>.
		/// <para>-or-</para>
		/// <para>Thrown when a buffer overrun is detected because the size, in bytes, of the <paramref name="count"/> is too large for the buffer.</para>
		/// </exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		void WriteRange<T>(T[] array, int index, int count)
			where T : struct;

		/// <summary>
		/// Function to copy data from the specified buffer into this buffer. 
		/// </summary>
		/// <param name="buffer">The buffer to copy the data from.</param>
		/// <param name="sourceOffset">The offset in the source buffer to start copying from, in bytes.</param>
		/// <param name="sourceSize">The number of bytes to copy from the source buffer.</param>
		/// <param name="destinationOffset">[Optional] The offset in this buffer to start copying to, in bytes.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceOffset"/>, <paramref name="sourceSize"/>, or the <paramref name="destinationOffset"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="sourceOffset"/> plus the <paramref name="sourceSize"/> exceeds the size of the source <paramref name="buffer"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="destinationOffset"/> plus the <paramref name="sourceSize"/> exceeds the size of the destination buffer.</para>
		/// </exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		void CopyFrom(IGorgonPointer buffer, long sourceOffset, int sourceSize, long destinationOffset = 0);

		/// <summary>
		/// Function to copy data from the specified buffer into this buffer. 
		/// </summary>
		/// <param name="buffer">The buffer to copy the data from.</param>
		/// <param name="sourceSize">The number of bytes to copy from the source buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceSize"/> parameter is less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="sourceSize"/> will exceeds the size of the source <paramref name="buffer"/> or the destination buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		void CopyFrom(IGorgonPointer buffer, int sourceSize);

		/// <summary>
		/// Function to copy data from unmanaged memory pointed at by the <see cref="IntPtr"/> into this buffer.
		/// </summary>
		/// <param name="source">The pointer to copy the data from.</param>
		/// <param name="size">The number of bytes to copy from the source buffer.</param>
		/// <param name="destinationOffset">[Optional] The offset in this buffer to start copying to, in bytes.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <see cref="IntPtr.Zero"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="destinationOffset"/> parameter is less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="destinationOffset"/> plus the <paramref name="size"/> exceeds the size of the destination buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		void CopyMemory(IntPtr source, int size, long destinationOffset = 0);

		/// <summary>
		/// Function to copy data from unmanaged memory pointed at by the pointer into this buffer.
		/// </summary>
		/// <param name="source">The pointer to copy the data from.</param>
		/// <param name="size">The number of bytes to copy from the source buffer.</param>
		/// <param name="destinationOffset">[Optional] The offset in this buffer to start copying to, in bytes.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <see cref="IntPtr.Zero"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="destinationOffset"/> parameter is less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="destinationOffset"/> plus the <paramref name="size"/> exceeds the size of the destination buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		unsafe void CopyMemory(void* source, int size, long destinationOffset = 0);

		/// <summary>
		/// Function to fill this buffer with the specified byte value.
		/// </summary>
		/// <param name="value">The value to fill the buffer with.</param>
		/// <param name="size">The number of bytes to fill.</param>
		/// <param name="offset">[Optional] The offset to start filling at, in bytes.</param>
		void Fill(byte value, int size, long offset = 0);

		/// <summary>
		/// Function to fill this buffer with the specified byte value.
		/// </summary>
		/// <param name="value">The value to fill the buffer with.</param>
		void Fill(byte value);

		/// <summary>
		/// Function to fill this buffer with zeroes.
		/// </summary>
		/// <param name="size">The number of bytes to fill.</param>
		/// <param name="offset">[Optional] The offset to start filling at, in bytes.</param>
		void Zero(int size, long offset = 0);

		/// <summary>
		/// Function to fill this buffer with zeroes.
		/// </summary>
		void Zero();

		/// <summary>
		/// Function to create a new <see cref="GorgonDataStream"/> from this pointer.
		/// </summary>
		/// <returns>The data stream wrapping this pointer.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the <see cref="Size"/> is larger than 2GB when running as an x86 application.</exception>
		GorgonDataStream ToDataStream();

		/// <summary>
		/// Function to copy the memory pointed at by this object into a <see cref="MemoryStream"/>
		/// </summary>
		/// <returns>The <see cref="MemoryStream"/> containing a copy of the data in memory that is pointed at by this object.</returns>
		MemoryStream CopyToMemoryStream();
	}
}