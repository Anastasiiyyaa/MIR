using System.Xml.Linq;

namespace xml.Lib
{
    public class XmlCalculate : IXmlCalculate
    {
        public (int objectsCount, int propertiesCount) Calculate(Stream xmlStream)
        {
            if (xmlStream == null)
                throw new ArgumentNullException(nameof(xmlStream), XmlConstants.ErrNullStream);

            var doc = XDocument.Load(xmlStream);
            var root = doc.Root
                       ?? throw new InvalidOperationException(XmlConstants.ErrEmptyDocument);

            if (root.Name.LocalName == XmlConstants.LinkXml &&
                string.Equals(root.Attribute(XmlConstants.Format)?.Value,
                              XmlConstants.FormatValueLinkXml,
                              StringComparison.Ordinal))
            {
                var summary = root.Element(XmlConstants.Summary)
                              ?? throw new InvalidOperationException(XmlConstants.ErrMissingSummary);

                var oAttr = summary.Attribute(XmlConstants.ObjectsCount)
                              ?? throw new InvalidOperationException(XmlConstants.ErrMissingObjectsCountAttr);
                var pAttr = summary.Attribute(XmlConstants.PropertiesCount)
                              ?? throw new InvalidOperationException(XmlConstants.ErrMissingPropertiesCountAttr);

                return ((int)oAttr, (int)pAttr);
            }

            var objs = root.Element(XmlConstants.Objects)
                       ?? throw new InvalidOperationException(XmlConstants.ErrMissingObjects);

            return (
                objectsCount: objs.Elements(XmlConstants.Object).Count(),
                propertiesCount: objs.Elements(XmlConstants.Property).Count()
            );
        }
    }
}
