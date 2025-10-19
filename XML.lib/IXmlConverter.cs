namespace xml.Lib
{
    public interface IXmlConverter
    {
        IEnumerable<Stream> Convert(Stream bigXmlStream);
        XmlFileType DetectFileType(Stream xmlStream);
    }
}
