using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EFCore.Sharding.Config
{
    internal static class Cache
    {
        public static IServiceProvider RootServiceProvider { get; set; }
        public static readonly ConcurrentDictionary<GenericDbContext, GenericDbContext> DbContexts = new ConcurrentDictionary<GenericDbContext, GenericDbContext>();
    }
}
