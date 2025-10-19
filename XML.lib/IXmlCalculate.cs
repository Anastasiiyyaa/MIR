namespace xml.Lib
{
    public interface IXmlCalculate
    {
        (int objectsCount, int propertiesCount) Calculate(Stream xmlStream);
    }
}
