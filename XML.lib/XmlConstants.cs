namespace xml.Lib
{
    internal static class XmlConstants
    {
        public const string LinkXml = "LinkXML";
        public const string LinkIndex = "LinkIndex";
        public const string Objects = "Objects";
        public const string Object = "Object";
        public const string Property = "Property";
        public const string Link = "Link";
        public const string Summary = "Summary";
        public const string Files = "Files";
        public const string Relationships = "Relationships";
        public const string File = "File";

        public const string Format = "format";
        public const string Version = "version";
        public const string ObjectsCount = "objectsCount";
        public const string PropertiesCount = "propertiesCount";
        public const string Filename = "filename";
        public const string ObjectId = "id";
        public const string FromId = "fromId";
        public const string ToId = "toId";
        public const string Relation = "relation";
        public const string OwnerId = "ownerId";


        public const string FormatValueLinkXml = "LinkXML";
        public const string FormatValueLinkIndex = "LinkIndex";
        public const string LinkXmlVersionValue = "1.0";

        public const string ErrNullStream = "Поток не может быть null";
        public const string ErrEmptyDocument = "Пустой документ";
        public const string ErrMissingSummary = "Нет <Summary>";
        public const string ErrMissingObjects = "Нет <Objects>";
        public const string ErrMissingObjectsCountAttr = "Нет атрибута objectsCount в <Summary>";
        public const string ErrMissingPropertiesCountAttr = "Нет атрибута propertiesCount в <Summary>";
        public const string ErrUnsupportedFormat = "Неподдерживаемый формат: {0}";
    }
}
