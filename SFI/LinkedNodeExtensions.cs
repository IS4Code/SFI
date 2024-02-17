using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;

namespace IS4.SFI
{
    /// <summary>
    /// Provides extension methods for <see cref="ILinkedNode"/>.
    /// </summary>
    public static class LinkedNodeExtensions
    {
        /// <summary>
        /// Sets the members of <paramref name="node"/> to individual resources.
        /// </summary>
        /// <param name="node">The <see cref="ILinkedNode"/> instance to assign the values to.</param>
        /// <param name="values">The individual member values to assign.</param>
        public static void SetMembers(this ILinkedNode node, IEnumerable<IndividualUri> values)
        {
            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, value);
            }
        }

        /// <summary>
        /// Sets the members of <paramref name="node"/> to plain literals.
        /// </summary>
        /// <param name="node"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{IndividualUri})" path="/param[@name='node']"/></param>
        /// <param name="values">The literal member values to assign.</param>
        public static void SetMembers(this ILinkedNode node, IEnumerable<string> values)
        {
            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, value);
            }
        }

        /// <summary>
        /// Sets the members of <paramref name="node"/> to literals with a particular datatype.
        /// </summary>
        /// <param name="node"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{IndividualUri})" path="/param[@name='node']"/></param>
        /// <param name="datatype">The datatype of the literals.</param>
        /// <param name="values"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{string})" path="/param[@name='values']"/></param>
        public static void SetMembers(this ILinkedNode node, IEnumerable<string> values, DatatypeUri datatype)
        {
            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, value, datatype);
            }
        }

        /// <summary>
        /// Sets the members of <paramref name="node"/> to literals with a datatype produced from a formatter.
        /// </summary>
        /// <typeparam name="TData">The type supported by <paramref name="datatypeFormatter"/>.</typeparam>
        /// <param name="node"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{IndividualUri})" path="/param[@name='node']"/></param>
        /// <param name="datatypeFormatter">The formatter to use for the datatype.</param>
        /// <param name="datatypeValue">The value to format for the datatype.</param>
        /// <param name="values"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{string})" path="/param[@name='values']"/></param>
        public static void SetMembers<TData>(this ILinkedNode node, IEnumerable<string> values, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue)
        {
            var datatype = datatypeFormatter[datatypeValue];
            if(datatype == null)
            {
                return;
            }

            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, value, UriFormatter.Instance, datatype);
            }
        }

        /// <summary>
        /// Sets the members of <paramref name="node"/> to the string values of literals with a particular datatype.
        /// </summary>
        /// <typeparam name="TValue">The type of the literals.</typeparam>
        /// <inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{string}, DatatypeUri)"/>
        public static void SetMembers<TValue>(this ILinkedNode node, IEnumerable<TValue> values, DatatypeUri datatype) where TValue : IFormattable
        {
            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, value, datatype);
            }
        }

        /// <summary>
        /// Sets the members of <paramref name="node"/> to the strings value of literals with a datatype produced from a formatter.
        /// </summary>
        /// <typeparam name="TData">The type supported by <paramref name="datatypeFormatter"/>.</typeparam>
        /// <param name="node"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{IndividualUri})" path="/param[@name='node']"/></param>
        /// <param name="datatypeFormatter">The formatter to use for the datatype.</param>
        /// <param name="datatypeValue">The value to format for the datatype.</param>
        /// <param name="values"><inheritdoc cref="SetMembers{TValue}(ILinkedNode, IEnumerable{TValue}, DatatypeUri)" path="/param[@name='values']"/></param>
        /// <typeparam name="TValue"><inheritdoc cref="SetMembers{TValue}(ILinkedNode, IEnumerable{TValue}, DatatypeUri)" path="/typeparam[@name='TValue']"/></typeparam>
        public static void SetMembers<TValue, TData>(this ILinkedNode node, IEnumerable<TValue> values, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable
        {
            var datatype = datatypeFormatter[datatypeValue];
            if(datatype == null)
            {
                return;
            }

            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, value, UriFormatter.Instance, datatype);
            }
        }

        /// <summary>
        /// Sets the members of <paramref name="node"/> to literals with a particular language.
        /// </summary>
        /// <param name="node"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{IndividualUri})" path="/param[@name='node']"/></param>
        /// <param name="language">The language of the literals.</param>
        /// <param name="values"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{string})" path="/param[@name='values']"/></param>
        public static void SetMembers(this ILinkedNode node, IEnumerable<string> values, LanguageCode language)
        {
            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, value, language);
            }
        }

        /// <summary>
        /// Sets the members of <paramref name="node"/> to individuals produced from a formatter.
        /// </summary>
        /// <typeparam name="TValue">The type of <paramref name="values"/>.</typeparam>
        /// <param name="node"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{IndividualUri})" path="/param[@name='node']"/></param>
        /// <param name="formatter">The formatter to use.</param>
        /// <param name="values">The value to format.</param>
        public static void SetMembers<TValue>(this ILinkedNode node, IIndividualUriFormatter<TValue> formatter, IEnumerable<TValue> values)
        {
            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, formatter, value);
            }
        }

        /// <summary>
        /// Sets the members of <paramref name="node"/> to URI literals.
        /// </summary>
        /// <param name="node"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{IndividualUri})" path="/param[@name='node']"/></param>
        /// <param name="values">The URI member values to assign.</param>
        public static void SetMembers(this ILinkedNode node, IEnumerable<Uri> values)
        {
            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, value);
            }
        }

        /// <summary>
        /// Sets the members of <paramref name="node"/> to resources identified by another <see cref="ILinkedNode"/>.
        /// </summary>
        /// <param name="node"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{IndividualUri})" path="/param[@name='node']"/></param>
        /// <param name="values">The node to assign.</param>
        public static void SetMembers(this ILinkedNode node, IEnumerable<ILinkedNode> values)
        {
            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, value);
            }
        }

        /// <summary>
        /// Sets the members of <paramref name="node"/> to boolean values.
        /// </summary>
        /// <param name="node"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{IndividualUri})" path="/param[@name='node']"/></param>
        /// <param name="values">The boolean member values to assign.</param>
        public static void SetMembers(this ILinkedNode node, IEnumerable<bool> values)
        {
            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, value);
            }
        }

        /// <summary>
        /// Sets the members of <paramref name="node"/> to literal values with an automatically recognized type.
        /// </summary>
        /// <typeparam name="TValue">The type of the literals.</typeparam>
        /// <param name="node"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{IndividualUri})" path="/param[@name='node']"/></param>
        /// <param name="values">The member values to assign.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="values"/> doesn't have a recognized datatype.
        /// </exception>
        public static void SetMembers<TValue>(this ILinkedNode node, IEnumerable<TValue> values) where TValue : struct, IEquatable<TValue>, IFormattable
        {
            int index = 0;
            foreach(var value in values)
            {
                node.Set(Properties.MemberAt, ++index, value);
            }
        }

        /// <inheritdoc cref="TrySetMembers(ILinkedNode, IEnumerable{ValueType})"/>
        public static int TrySetMembers(this ILinkedNode node, IEnumerable<object?> values)
        {
            int count = 0;

            int index = 0;
            foreach(var value in values)
            {
                if(node.TrySet(Properties.MemberAt, ++index, value))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Attempts to set the members of <paramref name="node"/> to values based on their runtime type.
        /// </summary>
        /// <param name="node"><inheritdoc cref="SetMembers(ILinkedNode, IEnumerable{IndividualUri})" path="/param[@name='node']"/></param>
        /// <param name="values">The member values to assign.</param>
        /// <returns>The number of values from <paramref name="values"/> whose assignment was successful.</returns>
        public static int TrySetMembers(this ILinkedNode node, IEnumerable<ValueType> values)
        {
            int count = 0;

            int index = 0;
            foreach(var value in values)
            {
                if(node.TrySet(Properties.MemberAt, ++index, value))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
