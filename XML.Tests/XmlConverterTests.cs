using System;
using System.IO;
using System.Linq;
using Xunit;
using XML.lib;

namespace XML.Tests
{
    public class XmlConverterTests
    {
        private readonly IXmlConverter  _converter   = new XmlConverter();
        private readonly IXmlCalculator _calculator  = new XmlCalculator();

        [Fact]
        public void Convert_And_Calculate_MatchBigXmlSummary()
        {
            // Ищем в выходной папке XML-файл с именем big.xml независимо от регистра
            var baseDir = AppContext.BaseDirectory;
            var bigFilePath = Directory
                .EnumerateFiles(baseDir, "*.xml")
                .FirstOrDefault(f =>
                    Path.GetFileName(f)
                        .Equals("big.xml", StringComparison.OrdinalIgnoreCase)
                );

            Assert.False(string.IsNullOrEmpty(bigFilePath), 
                $"Не найден файл big.xml в {baseDir}");

            using var bigStream = File.OpenRead(bigFilePath);

            // Конвертируем
            var streams = _converter.Convert(bigStream).ToList();

            // Считаем по оригиналу
            bigStream.Position = 0;
            var (origObjects, origProps) = _calculator.Calculate(bigStream);

            // LinkXML — последний
            var linkStream = streams.Last();
            linkStream.Position = 0;
            var (linkObjects, linkProps) = _calculator.Calculate(linkStream);

            Assert.Equal(origObjects, linkObjects);
            Assert.Equal(origProps,   linkProps);
        }
    }
}
