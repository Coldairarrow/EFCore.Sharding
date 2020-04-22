using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Sharding.DataAnnotations
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IndexAttribute : Attribute
    {
        public IndexAttribute()
        {

        }
    }
}
