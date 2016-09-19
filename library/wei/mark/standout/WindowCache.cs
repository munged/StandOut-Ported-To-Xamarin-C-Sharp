using System;
using System.Collections.Generic;

namespace wei.mark.standout
	{


	using Window = ui.Window;

	using Android.Util;

	public class WindowCache
		{
		public IDictionary<Type, SparseArray<Window>> SWindows;

		public WindowCache()
			{
			SWindows = new Dictionary<Type, SparseArray<Window>>();
			}

		/// <summary>
		/// Returns whether the window corresponding to the class and id exists in
		/// the <seealso cref="#sWindows"/> cache.
		/// </summary>
		/// <param name="id">
		///            The id representing the window. </param>
		/// <param name="cls">
		///            Class corresponding to the window. </param>
		/// <returns> True if the window corresponding to the class and id exists in
		///         the cache, or false if it does not exist. </returns>
		public virtual bool IsCached(int id, Type cls)
			{
			return GetCache(id, cls) != null;
			}

		/// <summary>
		/// Returns the window corresponding to the id from the <seealso cref="#sWindows"/>
		/// cache.
		/// </summary>
		/// <param name="id">
		///            The id representing the window. </param>
		/// <param name="cls">
		///            The class of the implementation of the window. </param>
		/// <returns> The window corresponding to the id if it exists in the cache, or
		///         null if it does not. </returns>
		public virtual Window GetCache(int id, Type cls)
			{
			SparseArray<Window> l2 = SWindows.GetValueOrNull(cls);
			if (l2 == null)
				{
				return null;
				}

			return l2.Get(id);
			}

		/// <summary>
		/// Add the window corresponding to the id in the <seealso cref="#sWindows"/> cache.
		/// </summary>
		/// <param name="id">
		///            The id representing the window. </param>
		/// <param name="cls">
		///            The class of the implementation of the window. </param>
		/// <param name="window">
		///            The window to be put in the cache. </param>
		public virtual void PutCache(int id, Type cls, Window window)
			{
			SparseArray<Window> l2 = SWindows.GetValueOrNull(cls);
			if (l2 == null)
				{
				l2 = new SparseArray<Window>();
				SWindows[cls] = l2;
				}

			l2.Put(id, window);
			}

		/// <summary>
		/// Remove the window corresponding to the id from the <seealso cref="#sWindows"/>
		/// cache.
		/// </summary>
		/// <param name="id">
		///            The id representing the window. </param>
		/// <param name="cls">
		///            The class of the implementation of the window. </param>
		public virtual void RemoveCache(int id, Type cls)
			{
			SparseArray<Window> l2 = SWindows.GetValueOrNull(cls);
			if (l2 != null)
				{
				l2.Remove(id);
				if (l2.Size() == 0)
					{
					SWindows.Remove(cls);
					}
				}
			}

		/// <summary>
		/// Returns the size of the <seealso cref="#sWindows"/> cache.
		/// </summary>
		/// <returns> True if the cache corresponding to this class is empty, false if
		///         it is not empty. </returns>
		/// <param name="cls">
		///            The class of the implementation of the window. </param>
		public virtual int GetCacheSize(Type cls)
			{
			SparseArray<Window> l2 = SWindows.GetValueOrNull(cls);
			if (l2 == null)
				{
				return 0;
				}

			return l2.Size();
			}

		/// <summary>
		/// Returns the ids in the <seealso cref="#sWindows"/> cache.
		/// </summary>
		/// <param name="cls">
		///            The class of the implementation of the window. </param>
		/// <returns> The ids representing the cached windows. </returns>
		public virtual ISet<int?> GetCacheIds(Type cls)
			{
			SparseArray<Window> l2 = SWindows.GetValueOrNull(cls);
			if (l2 == null)
				{
				return new HashSet<int?>();
				}

			ISet<int?> keys = new HashSet<int?>();
			for (int i = 0; i < l2.Size(); i++)
				{
				keys.Add(l2.KeyAt(i));
				}
			return keys;
			}

		public virtual int Size()
			{
			return SWindows.Count;
			}
		}
	}
