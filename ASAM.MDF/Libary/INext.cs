using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace ASAM.MDF.Libary
{
    public interface INext<T>
        where T : INext<T>
    {
        T Next { get; }
    }
    public interface IPrevious<T> where T : IPrevious<T>
    {
        T Previous { get; set; }
    }
}
