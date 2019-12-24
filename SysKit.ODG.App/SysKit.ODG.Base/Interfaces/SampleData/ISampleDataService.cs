using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SysKit.ODG.Base.Interfaces.SampleData
{
    public interface ISampleDataService
    {
        ReadOnlyCollection<string> FirstNames { get; }
        ReadOnlyCollection<string> LastNames { get; }

        string GetRandomValue(IList<string> sampleCollection);
        string GetRandomValue(IList<string> primaryCollection, IList<string> secondaryCollection);
        string GetRandomValue(IList<string> primaryCollection, IList<string> secondaryCollection, IList<string> ternaryCollection);
    }
}
