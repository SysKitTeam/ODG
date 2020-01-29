using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SysKit.ODG.Base.XmlTemplate;

namespace SysKit.ODG.Generation
{
    public class XmlSpecificationService
    {
        public void SerializeSpecification<T>(T specification, string specificationFile) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new FileStream(specificationFile, FileMode.Create))
            {
                serializer.Serialize(writer, specification);
            }
        }

        public T DeserializeSpecification<T>(string specificationFile) where T: class
        {
            T specification;
            var serializer = new XmlSerializer(typeof(T));
           
            using (var reader = XmlReader.Create(specificationFile))
            {
                specification = serializer.Deserialize(reader) as T;
            }

            return specification;
        }
    }
}
