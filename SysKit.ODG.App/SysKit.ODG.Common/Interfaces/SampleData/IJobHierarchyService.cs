namespace SysKit.ODG.Common.Interfaces.SampleData
{
    public interface IJobHierarchyService
    {
        (int hierarchyLevel, string jobTitle) GetHierarchyLevelAndJobTitle(string company, string department);
    }
}