using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SysKit.ODG.XMLSpecification.Model;

namespace SysKit.ODG.XMLSpecification
{
    public class XmlSpecificationService
    {
        public void SerializeSpecification(XmlODGSpecification specification, string specificationFile)
        {
            var serializer = new XmlSerializer(typeof(XmlODGSpecification));
            using (var writer = new FileStream(specificationFile, FileMode.Create))
            {
                serializer.Serialize(writer, specification);
            }
        }

        public XmlODGSpecification DeserializeSpecification(string specificationFile)
        {
            XmlODGSpecification specification;
            var serializer = new XmlSerializer(typeof(XmlODGSpecification));
           
            using (var reader = XmlReader.Create(specificationFile))
            {
                specification = serializer.Deserialize(reader) as XmlODGSpecification;
            }

            return specification;
        }
    }
}
