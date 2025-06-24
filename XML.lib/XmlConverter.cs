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

            // Загружаем документ из входного потока
            XDocument bigDoc = XDocument.Load(bigXmlStream);

            // Извлекаем контейнер объектов
            XElement objectsElement = bigDoc.Root.Element("Objects");
            if (objectsElement == null)
                throw new InvalidOperationException("BigXML не содержит элемента 'Objects'.");

            // Получаем список объектов
            List<XElement> objects = objectsElement.Elements("Object").ToList();

            // Извлекаем связи (если заданы)
            XElement linksElement = bigDoc.Root.Element("Links");
            List<XElement> links = linksElement != null ? linksElement.Elements("Link").ToList() : null;

            List<Stream> resultStreams = new List<Stream>();
            List<(string filename, string objectId)> fileInfos = new List<(string filename, string objectId)>();

            int fileCounter = 1;
            foreach (var obj in objects)
            {
                // Используем атрибут id или порядковый номер
                string objectId = (string)obj.Attribute("id") ?? fileCounter.ToString();

                // Создаём документ для отдельного объекта
                XDocument objDoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Object", obj.Elements())
                );

                MemoryStream objStream = new MemoryStream();
                objDoc.Save(objStream);
                objStream.Position = 0;
                resultStreams.Add(objStream);

                fileInfos.Add((filename: $"File{fileCounter}.xml", objectId: objectId));
                fileCounter++;
            }

            // Вычисляем агрегированные данные
            int totalObjects = objects.Count;
            int totalProperties = objects.Sum(o => o.Descendants("Property").Count());

            // Строим файл-ссылку LinkXML
            XDocument linkDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("LinkXML",
                    new XAttribute("format", "LinkXML"),
                    new XAttribute("version", "1.0"),
                    new XElement("Summary",
                        new XAttribute("objectsCount", totalObjects),
                        new XAttribute("propertiesCount", totalProperties)
                    ),
                    new XElement("Files",
                        fileInfos.Select(fi =>
                            new XElement("File",
                                new XAttribute("filename", fi.filename),
                                new XAttribute("objectId", fi.objectId)
                            )
                        )
                    ),
                    links != null ?
                        new XElement("Relationships",
                            links.Select(l =>
                                new XElement("Link",
                                    new XAttribute("from", (string)l.Attribute("from")),
                                    new XAttribute("to", (string)l.Attribute("to"))
                                )
                            )
                        ) : null
                )
            );

            MemoryStream linkStream = new MemoryStream();
            linkDoc.Save(linkStream);
            linkStream.Position = 0;
            resultStreams.Add(linkStream);

            return resultStreams;
        }
    }
}
