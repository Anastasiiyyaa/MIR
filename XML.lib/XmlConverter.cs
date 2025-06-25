using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace XML.lib
{
    public class XmlConverter : IXmlConverter
    {
        public IEnumerable<Stream> Convert(Stream bigXmlStream)
        {
            if (bigXmlStream == null) 
                throw new ArgumentNullException(nameof(bigXmlStream));

            var doc = XDocument.Load(bigXmlStream);
            var objectsContainer = doc.Root.Element("Objects")
                ?? throw new InvalidOperationException("Нет тега <Objects>");

            // собираем все элементы
            var objectElems   = objectsContainer.Elements("Object").ToList();
            var propertyElems = objectsContainer.Elements("Property").ToList();
            var linkElems     = objectsContainer.Elements("Link").ToList();

            var result = new List<Stream>();
            var fileInfos = new List<(string Filename, string ObjectId)>();

            int idx = 1;
            foreach (var obj in objectElems)
            {
                // id объекта
                var objectId = (string)obj.Attribute("id") 
                               ?? idx.ToString();

                // свойства, принадлежащие этому объекту
                var myProps = propertyElems
                    .Where(p => (string)p.Attribute("ownerId") == objectId)
                    .ToList();

                // связи, где участвует этот объект
                var myLinks = linkElems
                    .Where(l =>
                        (string)l.Attribute("fromId") == objectId
                     || (string)l.Attribute("toId")   == objectId
                    ).ToList();

                // строим отдельный файл
                var subDoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Object",
                        // атрибуты исходного <Object>
                        obj.Attributes(),
                        // подставляем его свойства
                        myProps.Select(p => new XElement("Property", p.Attributes())),
                        // подставляем связи
                        myLinks.Select(l => new XElement("Link", l.Attributes()))
                    )
                );

                var ms = new MemoryStream();
                subDoc.Save(ms);
                ms.Position = 0;
                result.Add(ms);

                fileInfos.Add(($"Object_{objectId}.xml", objectId));
                idx++;
            }

            // собираем summary
            int totalObjects    = objectElems.Count;
            int totalProperties = propertyElems.Count;
            // все ссылки оставляем в одном LinkXML
            var linkXml = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("LinkXML",
                    new XAttribute("format", "LinkXML"),
                    new XAttribute("version", "1.0"),
                    new XElement("Summary",
                        new XAttribute("objectsCount",    totalObjects),
                        new XAttribute("propertiesCount", totalProperties)
                    ),
                    new XElement("Files",
                        fileInfos.Select(fi =>
                            new XElement("File",
                                new XAttribute("filename", fi.Filename),
                                new XAttribute("objectId", fi.ObjectId)
                            )
                        )
                    ),
                    new XElement("Relationships",
                        linkElems.Select(l =>
                            new XElement("Link",
                                new XAttribute("fromId",   (string)l.Attribute("fromId")),
                                new XAttribute("toId",     (string)l.Attribute("toId")),
                                new XAttribute("relation", (string)l.Attribute("relation"))
                            )
                        )
                    )
                )
            );

            var linkStream = new MemoryStream();
            linkXml.Save(linkStream);
            linkStream.Position = 0;
            result.Add(linkStream);

            return result;
        }
    }
}
