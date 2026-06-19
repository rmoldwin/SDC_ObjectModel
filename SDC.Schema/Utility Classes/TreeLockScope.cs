using System.Threading;

namespace SDC.Schema
{
    /// <summary>
    /// Acquires a shared READ lock on <see cref="_ITopNode.TreeRwLock"/> for the duration of a <c>using</c> scope.
    /// Allocation-free (<c>ref struct</c>); the compiler rejects <c>await</c> inside a <c>using</c> block that
    /// holds this scope, enforcing the "no lock across await" rule at compile time.
    /// </summary>
    /// <remarks>
    /// Construct once at the public entry point of a read operation; internal helpers called under the same
    /// lock must NOT acquire it again (the lock is created with
    /// <see cref="LockRecursionPolicy.SupportsRecursion"/> so read-in-read is safe, but adding extra
    /// acquire/release pairs in helpers is wasteful and error-prone).
    /// </remarks>
    internal readonly ref struct ReadLockScope
    {
        private readonly ReaderWriterLockSlim _lock;

        /// <summary>
        /// Enters the read lock on <paramref name="rwLock"/>.
        /// </summary>
        /// <param name="rwLock">The per-<see cref="ITopNode"/> <see cref="ReaderWriterLockSlim"/>.</param>
        internal ReadLockScope(ReaderWriterLockSlim rwLock)
        {
            _lock = rwLock;
            _lock.EnterReadLock();
        }

        /// <summary>Exits the read lock.</summary>
        public void Dispose() => _lock.ExitReadLock();
    }

    /// <summary>
    /// Acquires an exclusive WRITE lock on <see cref="_ITopNode.TreeRwLock"/> for the duration of a <c>using</c> scope.
    /// Allocation-free (<c>ref struct</c>); the compiler rejects <c>await</c> inside a <c>using</c> block that
    /// holds this scope.
    /// </summary>
    /// <remarks>
    /// Writers must acquire this lock at the TOP of each public mutation entry point before any reads or writes.
    /// Internal helpers (e.g., <c>RegisterIn_*</c>, <c>UnRegisterIn_*</c>) run under the already-held write
    /// lock and must NOT acquire it again.
    /// NEVER take a <see cref="ReadLockScope"/> and then attempt to upgrade to a <see cref="WriteLockScope"/>
    /// on the same thread — that is the classic <see cref="ReaderWriterLockSlim"/> deadlock.
    /// </remarks>
    internal readonly ref struct WriteLockScope
    {
        private readonly ReaderWriterLockSlim _lock;

        /// <summary>
        /// Enters the write lock on <paramref name="rwLock"/>.
        /// </summary>
        /// <param name="rwLock">The per-<see cref="ITopNode"/> <see cref="ReaderWriterLockSlim"/>.</param>
        internal WriteLockScope(ReaderWriterLockSlim rwLock)
        {
            _lock = rwLock;
            _lock.EnterWriteLock();
        }

        /// <summary>Exits the write lock.</summary>
        public void Dispose() => _lock.ExitWriteLock();
    }

    /// <summary>
    /// Extension methods that make it easy to obtain a nullable <see cref="ReaderWriterLockSlim"/>
    /// from any <see cref="BaseType"/> node without repeating the <c>TopNode</c> null-guard.
    /// </summary>
    internal static class TreeRwLockExtensions
    {
        /// <summary>
        /// Returns the <see cref="ReaderWriterLockSlim"/> for the tree that owns <paramref name="node"/>,
        /// or <see langword="null"/> when the node has no <see cref="ITopNode"/> yet
        /// (i.e., it was constructed without a parent and has not yet been attached to a tree).
        /// </summary>
        /// <remarks>
        /// Callers should skip locking when this returns <see langword="null"/>: the node is not yet
        /// reachable by any other thread via the shared dictionaries.
        /// </remarks>
        internal static ReaderWriterLockSlim? RwLockOrNull(this BaseType node)
            => node.TopNode is _ITopNode itn ? itn.TreeRwLock : null;
    }
}
