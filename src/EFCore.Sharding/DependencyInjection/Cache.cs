﻿using System;
using System.Collections.Generic;

namespace EFCore.Sharding.Config
{
    internal static class Cache
    {
        public static IServiceProvider RootServiceProvider { get; set; }
        public static readonly SynchronizedCollection<GenericDbContext> DbContexts
            = new SynchronizedCollection<GenericDbContext>();
    }
}
