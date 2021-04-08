using IS4.MultiArchiver.Vocabulary;
using System;

namespace IS4.MultiArchiver.Services
{
    public interface ILinkedNode
    {
        void Set(Classes @class);
        void Set(Properties property, Individuals value);
        void Set(Properties property, string value);
        void Set(Properties property, string value, Datatypes datatype);
        void Set(Properties property, Uri value);
        void Set(Properties property, ILinkedNode value);
        void Set<T>(Properties property, T value) where T : struct;
    }
}
