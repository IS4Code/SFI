﻿using System;

namespace IS4.MultiArchiver.Services
{
    public interface ILinkedNodeFactory
    {
        ILinkedNode Root { get; }
        ILinkedNode Create<T>(IUriFormatter<T> formatter, T value);
        ILinkedNode Create<T>(T entity) where T : class;
    }
}