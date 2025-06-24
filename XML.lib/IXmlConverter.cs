using System.Collections.Generic;
using System.IO;

namespace XML.lib
{
    public interface IXmlConverter
    {
        /// <summary>
        /// Преобразует поток большого XML-файла (BigXML) в набор потоков – по одному файлу на объект,
        /// последний поток представляет файл-ссылку LinkXML.
        /// </summary>
        /// <param name="bigXmlStream">Поток исходного BigXML</param>
        /// <returns>Перечисление потоков с малыми XML-файлами</returns>
        IEnumerable<Stream> Convert(Stream bigXmlStream);
    }
}
