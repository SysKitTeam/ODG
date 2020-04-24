using System.Collections.Generic;

namespace SysKit.ODG.Base.DTO.Generation
{
    public interface ISharePointContent
    {
        string Url { get; }
        ContentEntry Content { get; set; }
    }
}
