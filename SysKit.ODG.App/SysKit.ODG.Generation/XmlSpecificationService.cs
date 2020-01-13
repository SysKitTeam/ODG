using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SysKit.ODG.Base.XmlTemplate;

namespace SysKit.ODG.Generation
{
    public class XmlSpecificationService
    {
        public void SerializeSpecification(XmlODGTemplate specification, string specificationFile)
        {
            var serializer = new XmlSerializer(typeof(XmlODGTemplate));
            using (var writer = new FileStream(specificationFile, FileMode.Create))
            {
                serializer.Serialize(writer, specification);
            }
        }

        public XmlODGTemplate DeserializeSpecification(string specificationFile)
        {
            XmlODGTemplate specification;
            var serializer = new XmlSerializer(typeof(XmlODGTemplate));
           
            using (var reader = XmlReader.Create(specificationFile))
            {
                specification = serializer.Deserialize(reader) as XmlODGTemplate;
            }

            return specification;
        }
    }
}
