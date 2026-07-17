using System.Collections;
using System.Collections.Generic;

namespace SDC.Schema
{
	/// <summary>
	/// Minimal thread-safe wrapper around <see cref="HashSet{T}"/>.<br/>
	/// All reads and mutations are serialized under a single private lock, so concurrent
	/// callers can never corrupt the underlying set (which throws
	/// <c>"Operations that change non-concurrent collections must have exclusive access"</c>
	/// when mutated from multiple threads).<br/><br/>
	/// HashSet semantics are preserved exactly, including null handling for reference types
	/// (e.g. <c>Add(null)</c> / <c>Remove(null)</c> behave as <see cref="HashSet{T}"/> does).<br/><br/>
	/// Introduced in Sprint F to fix a concurrent-construction race on the per-tree
	/// <c>_UniqueIDs</c> collection. The <c>ID</c> setter that mutates it lives in
	/// auto-generated code that cannot host its own locking, so thread-safety must be
	/// provided by the collection type itself.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	internal sealed class ThreadSafeSet<T> : IEnumerable<T>
	{
		private readonly HashSet<T> _set;
		private readonly object _gate = new();

		public ThreadSafeSet() => _set = new HashSet<T>();
		public ThreadSafeSet(IEqualityComparer<T>? comparer) => _set = new HashSet<T>(comparer);

		/// <summary>Adds an item. Returns <see langword="true"/> if it was not already present.</summary>
		public bool Add(T item)
		{
			lock (_gate) { return _set.Add(item); }
		}

		/// <summary>Removes an item. Returns <see langword="true"/> if it was present.</summary>
		public bool Remove(T item)
		{
			lock (_gate) { return _set.Remove(item); }
		}

		/// <summary>Determines whether the set contains <paramref name="item"/>.</summary>
		public bool Contains(T item)
		{
			lock (_gate) { return _set.Contains(item); }
		}

		/// <summary>
		/// Searches for an element equal to <paramref name="equalValue"/> and, if found,
		/// returns the stored instance in <paramref name="actualValue"/>.
		/// </summary>
		public bool TryGetValue(T equalValue, out T actualValue)
		{
			lock (_gate) { return _set.TryGetValue(equalValue, out actualValue!); }
		}

		/// <summary>Removes all elements.</summary>
		public void Clear()
		{
			lock (_gate) { _set.Clear(); }
		}

		/// <summary>Current element count (snapshot).</summary>
		public int Count
		{
			get { lock (_gate) { return _set.Count; } }
		}

		/// <summary>
		/// Returns an enumerator over a point-in-time snapshot, so enumeration is safe even
		/// while other threads mutate the set.
		/// </summary>
		public IEnumerator<T> GetEnumerator()
		{
			lock (_gate) { return new List<T>(_set).GetEnumerator(); }
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
