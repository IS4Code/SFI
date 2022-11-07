namespace IS4.SFI.Services
{
    /// <summary>
    /// Stores a pair of objects to server as a key when information
    /// about this instance need to be cached, usually via
    /// <see cref="IS4.SFI.Tools.PersistenceStore{TKey, TValue}"/>.
    /// This allows the instance to be reused or discarded as long as the
    /// <see cref="ReferenceKey"/> and <see cref="DataKey"/> unique identify
    /// the entity it describes.
    /// </summary>
    public interface IPersistentKey
    {
        /// <summary>
        /// The identity part of the key, intended to be compared by reference
        /// (i.e. via <see cref="System.Object.ReferenceEquals(object, object)"/>)
        /// during caching. The logical lifetime of this instance does not exceed the
        /// lifetime of this object.
        /// </summary>
        /// <example>
        /// For an entry in an archive, this property could point to the instance
        /// describing the archive itself (as long as the same instance is used
        /// for all its entries).
        /// </example>
        object? ReferenceKey { get; }

        /// <summary>
        /// The equality part of the key, intended to be compared by value
        /// (i.e. via <see cref="System.Object.Equals(object, object)"/>)
        /// during caching.
        /// </summary>
        /// <example>
        /// For an entry in an archive, this property could contain the path of the entry
        /// within the archive, as long as <see cref="ReferenceKey"/> identifies
        /// the archive itself.
        /// </example>
        object? DataKey { get; }
    }
}
