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

            XDocument doc = XDocument.Load(xmlStream);

            // Если это LinkXML, берем данные из Summary
            if (doc.Root.Name.LocalName == "LinkXML" &&
                doc.Root.Attribute("format")?.Value == "LinkXML")
            {
                XElement summary = doc.Root.Element("Summary");
                if (summary != null)
                {
                    int objCount = (int)summary.Attribute("objectsCount");
                    int propCount = (int)summary.Attribute("propertiesCount");
                    return (objCount, propCount);
                }
                return (0, 0);
            }
            else
            {
                // Если BigXML – ищем контейнер объектов
                XElement objectsElement = doc.Root.Element("Objects") ?? doc.Root;
                var objects = objectsElement.Elements("Object").ToList();
                int objCount = objects.Count;
                int propCount = objects.Sum(o => o.Descendants("Property").Count());
                return (objCount, propCount);
            }
        }
    }
}
