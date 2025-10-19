using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace XML.lib
{
    public class XmlCalculator : IXmlCalculator
    {
        public (int objectsCount, int propertiesCount) Calculate(Stream xmlStream)
        {
            if (xmlStream == null)
                throw new ArgumentNullException(nameof(xmlStream));

            var doc = XDocument.Load(xmlStream);
            var root = doc.Root;

            // Если это LinkXML — читаем Summary
            if (root.Name == "LinkXML" &&
                (string)root.Attribute("format") == "LinkXML")
            {
                var summary = root.Element("Summary");
                int o = (int)summary.Attribute("objectsCount");
                int p = (int)summary.Attribute("propertiesCount");
                return (o, p);
            }

            // Иначе предполагаем BigXML: внутри корня есть <Objects>
            var objsContainer = root.Element("Objects")
                ?? throw new InvalidOperationException("Нет <Objects>");

            int objCount = objsContainer.Elements("Object").Count();
            int propCount = objsContainer.Elements("Property").Count();
            return (objCount, propCount);
        }
    }
}
