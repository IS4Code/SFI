using System;

namespace IS4.SFI.Vocabulary
{
    /// <summary>
    /// Stores information about an RDF term from some vocabulary.
    /// </summary>
    public interface ITermUri
    {
        /// <summary>
        /// The vocabulary where the term resides.
        /// </summary>
        VocabularyUri Vocabulary { get; }

        /// <summary>
        /// The local name of the term inside the vocabulary.
        /// </summary>
        string Term { get; }

        /// <summary>
        /// The full value of the term, concatenated from <see cref="VocabularyUri.Value"/>
        /// and <see cref="Term"/>.
        /// </summary>
        string Value { get; }
    }

    /// <summary>
    /// Stores information about an RDF term from some vocabulary.
    /// </summary>
    /// <typeparam name="TSelf">The type of the term.</typeparam>
    public interface ITermUri<TSelf> : ITermUri, IEquatable<TSelf> where TSelf : ITermUri<TSelf>
    {

    }

    /// <summary>
    /// Stores information about an RDF term with directional information.
    /// </summary>
    public interface IDirectionalTermUri : ITermUri
    {
        /// <summary>
        /// Whether the term is to be applied in the inverse direction, as opposed to the normal one.
        /// </summary>
        bool IsInverse { get; }

        /// <summary>
        /// Produces a new term with the opposite direction.
        /// </summary>
        /// <returns>A new term with the negated value of <see cref="IsInverse"/>.</returns>
        IDirectionalTermUri AsInverse();
    }

    /// <summary>
    /// Stores information about an RDF term with directional information.
    /// </summary>
    /// <typeparam name="TSelf">The type of the term.</typeparam>
    public interface IDirectionalTermUri<TSelf> : ITermUri<TSelf>, IDirectionalTermUri where TSelf : ITermUri<TSelf>
    {

    }
}
