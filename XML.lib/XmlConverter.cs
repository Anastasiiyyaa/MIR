using System.Xml.Linq;

namespace xml.Lib
{
    public class XmlConverter : IXmlConverter
    {
        public IEnumerable<Stream> Convert(Stream bigXmlStream)
        {
            if (bigXmlStream == null)
                throw new ArgumentNullException(nameof(bigXmlStream), XmlConstants.ErrNullStream);

            var doc = XDocument.Load(bigXmlStream);
            if (GetXmlFileType(doc) != XmlFileType.BigXml)
                throw new InvalidOperationException(
                    string.Format(XmlConstants.ErrUnsupportedFormat, GetXmlFileType(doc)));

            var root = doc.Root
                       ?? throw new InvalidOperationException(XmlConstants.ErrEmptyDocument);

            var objectElems = GetObjectElements(root);
            var propertyElems = GetPropertyElements(root);
            var linkElems = GetLinkElements(root);

            var result = new List<Stream>();

            foreach (var obj in objectElems)
            {
                result.Add(BuildObjectFile(obj, propertyElems, linkElems));
            }

            result.Add(BuildLinkIndexFile(objectElems, propertyElems.Count, linkElems));

            return result;
        }

        private IEnumerable<XElement> GetObjectElements(XElement root) =>
            root.Element(XmlConstants.Objects)?
                .Elements(XmlConstants.Object)
                .ToList()
            ?? throw new InvalidOperationException(XmlConstants.ErrMissingObjects);

        private List<XElement> GetPropertyElements(XElement root) =>
            root.Element(XmlConstants.Objects)?
                .Elements(XmlConstants.Property)
                .ToList()
            ?? new List<XElement>();

        private List<XElement> GetLinkElements(XElement root) =>
            root.Element(XmlConstants.Objects)?
                .Elements(XmlConstants.Link)
                .ToList()
            ?? new List<XElement>();

        private Stream BuildObjectFile(
            XElement objElem,
            IEnumerable<XElement> allProps,
            IEnumerable<XElement> allLinks)
        {
            var id = objElem.Attribute(XmlConstants.ObjectId)?.Value
                     ?? throw new InvalidOperationException(
                            $"У объекта нет атрибута {XmlConstants.ObjectId}");

            var myProps = allProps
                .Where(p => p.Attribute(XmlConstants.OwnerId)?.Value == id);

            var myLinks = allLinks
                .Where(l =>
                    l.Attribute(XmlConstants.FromId)?.Value == id ||
                    l.Attribute(XmlConstants.ToId)?.Value == id);

            var subDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(XmlConstants.Object,
                    objElem.Attributes(),
                    myProps.Select(p => new XElement(XmlConstants.Property, p.Attributes())),
                    myLinks.Select(l => new XElement(XmlConstants.Link, l.Attributes()))
                )
            );

            var ms = new MemoryStream();
            subDoc.Save(ms);
            ms.Position = 0;
            return ms;
        }

        private Stream BuildLinkIndexFile(
            IEnumerable<XElement> objectElems,
            int totalProperties,
            IEnumerable<XElement> linkElems)
        {
            var files = objectElems
                .Select((o, i) =>
                {
                    var id = o.Attribute(XmlConstants.ObjectId)?.Value ?? (i + 1).ToString();
                    var filename = $"{XmlConstants.Object}_{id}.xml";
                    return new { filename, id };
                });

            var linkIndex = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(XmlConstants.LinkXml,
                    new XAttribute(XmlConstants.Format, XmlConstants.FormatValueLinkXml),
                    new XAttribute(XmlConstants.Version, XmlConstants.LinkXmlVersionValue),

                    new XElement(XmlConstants.Summary,
                        new XAttribute(XmlConstants.ObjectsCount, objectElems.Count()),
                        new XAttribute(XmlConstants.PropertiesCount, totalProperties)
                    ),

                    new XElement(XmlConstants.Files,
                        files.Select(f =>
                            new XElement(XmlConstants.File,
                                new XAttribute(XmlConstants.Filename, f.filename),
                                new XAttribute(XmlConstants.ObjectId, f.id)
                            )
                        )
                    ),

                    new XElement(XmlConstants.Relationships,
                        linkElems.Select(l =>
                            new XElement(XmlConstants.Link,
                                new XAttribute(XmlConstants.FromId, (string?)l.Attribute(XmlConstants.FromId) ?? ""),
                                new XAttribute(XmlConstants.ToId, (string?)l.Attribute(XmlConstants.ToId) ?? ""),
                                new XAttribute(XmlConstants.Relation, (string?)l.Attribute(XmlConstants.Relation) ?? "")
                            )
                        )
                    )
                )
            );

            var ms = new MemoryStream();
            linkIndex.Save(ms);
            ms.Position = 0;
            return ms;
        }

        public XmlFileType DetectFileType(Stream xmlStream)
        {
            if (xmlStream == null)
                throw new ArgumentNullException(nameof(xmlStream), XmlConstants.ErrNullStream);

            long originalPosition = 0;
            if (xmlStream.CanSeek)
                originalPosition = xmlStream.Position;

            XDocument doc = XDocument.Load(xmlStream);

            if (xmlStream.CanSeek)
                xmlStream.Position = originalPosition;

            return GetXmlFileType(doc);
        }

        private XmlFileType GetXmlFileType(XDocument doc)
        {
            var root = doc.Root;
            if (root == null)
                return XmlFileType.Unknown;

            if (root.Name.LocalName == XmlConstants.LinkXml &&
                root.Attribute(XmlConstants.Format)?.Value == XmlConstants.FormatValueLinkXml)
                return XmlFileType.LinkXml;

            if (root.Name.LocalName == XmlConstants.LinkIndex &&
                root.Attribute(XmlConstants.Format)?.Value == XmlConstants.FormatValueLinkIndex)
                return XmlFileType.LinkIndex;

            if (root.Element(XmlConstants.Objects) != null)
                return XmlFileType.BigXml;

            return XmlFileType.Unknown;
        }
    }
}
