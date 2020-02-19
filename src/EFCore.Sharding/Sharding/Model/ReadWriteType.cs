using System;

namespace EFCore.Sharding
{
    [Flags]
    public enum ReadWriteType
    {
        Read = 1,
        Write = 2
    }
}
