using System;
using System.Collections.Concurrent;

namespace FakeXrmEasy.Core
{
    internal class CachedReflectedType
    {
        internal Type ReflectedType { get; set; }
        internal readonly ConcurrentDictionary<string, Type> ReflectedAttributes = new ConcurrentDictionary<string, Type>();
    }
}