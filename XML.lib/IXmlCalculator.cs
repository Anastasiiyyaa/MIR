using System.IO;

namespace XML.lib
{
    public interface IXmlCalculator
    {
        /// <summary>
        /// Вычисляет количество объектов и свойств в XML-файле – как для BigXML, так и для LinkXML.
        /// </summary>
        /// <param name="xmlStream">Поток входного XML</param>
        /// <returns>(кол-во объектов, кол-во свойств)</returns>
        (int objectsCount, int propertiesCount) Calculate(Stream xmlStream);
    }
}
