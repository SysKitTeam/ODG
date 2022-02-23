using System.Collections.Generic;
using System.Collections.ObjectModel;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Interfaces.SampleData
{
    public interface ISampleDataService
    {
        ReadOnlyCollection<string> FirstNames { get; }
        ReadOnlyCollection<string> LastNames { get; }
        ReadOnlyCollection<string> GroupNames { get; }
        ReadOnlyCollection<string> DepartmentNames { get; }
        ReadOnlyCollection<string> CountryNames { get; }
        ReadOnlyCollection<string> CityNames { get; }

        string GetRandomValue(IList<string> sampleCollection);
        string GetRandomValue(IList<string> primaryCollection, IList<string> secondaryCollection);
        string GetRandomValue(IList<string> primaryCollection, IList<string> secondaryCollection, IList<string> ternaryCollection);

        RandomValueWithComponents GetRandomValueWithComponents(IList<string> primaryCollection, IList<string> secondaryCollection);
    }
}
