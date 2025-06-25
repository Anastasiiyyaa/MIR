using System;
using System.IO;
using System.Linq;

using Ionic.Zip;              // из DotNetZip

using XML.lib;

using Xunit;

namespace XML.Tests
{
    public class XmlConverterTests
    {
        private const string ZipFileName = "BIG.zip";
        private const string XmlFileName = "BIG.xml";
        private const string ZipPassword = "mir123"; // укажите здесь пароль

        private readonly IXmlConverter _converter = new XmlConverter();
        private readonly IXmlCalculator _calculator = new XmlCalculator();

        public XmlConverterTests()
        {
            EnsureBigXmlUnpacked();
        }

        [Fact]
        public void Convert_And_Calculate_MatchBigXmlSummary()
        {
            var baseDir = AppContext.BaseDirectory;
            var bigFilePath = Path.Combine(baseDir, XmlFileName);

            Assert.True(File.Exists(bigFilePath),
                $"Не найден {XmlFileName} в {baseDir}");

            using var bigStream = File.OpenRead(bigFilePath);

            // Конвертируем
            var streams = _converter.Convert(bigStream).ToList();

            // Считаем по оригиналу
            bigStream.Position = 0;
            var (origObjects, origProps) = _calculator.Calculate(bigStream);

            // Считаем по LinkXML — последнему потоку
            var linkStream = streams.Last();
            linkStream.Position = 0;
            var (linkObjects, linkProps) = _calculator.Calculate(linkStream);

            Assert.Equal(origObjects, linkObjects);
            Assert.Equal(origProps, linkProps);
        }



        /// <summary>
        /// Гарантирует, что big.xml распакован в выходной директории тестов.
        /// Если его там нет — распаковывает из защищённого ZIP.
        /// </summary>
        private static void EnsureBigXmlUnpacked()
        {
            var baseDir = AppContext.BaseDirectory;
            var xmlPath = Path.Combine(baseDir, XmlFileName);
            if (File.Exists(xmlPath))
                return;

            var zipPath = Path.Combine(baseDir, ZipFileName);
            if (!File.Exists(zipPath))
                throw new FileNotFoundException($"ZIP-файл не найден: {zipPath}");

            using (var zip = ZipFile.Read(zipPath))
            {
                zip.Password = ZipPassword;

                // Находим в архиве именно тот entry, который нам нужен
                var entry = zip.Entries
                    .FirstOrDefault(e =>
                        string.Equals(e.FileName, XmlFileName, StringComparison.OrdinalIgnoreCase));

                if (entry == null)
                    throw new InvalidDataException($"В архиве нет файла {XmlFileName}");

                // Распаковываем в папку тестов
                entry.Extract(baseDir, ExtractExistingFileAction.OverwriteSilently);
            }
        }
    }
}
