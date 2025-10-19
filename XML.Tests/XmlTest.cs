using System.Reflection;
using System.Text;
using System.Xml.Linq;

using Ionic.Zip;

using xml.Lib;
using Xunit.Abstractions;

namespace xml.Tests
{
    public class XmlConverterTests
    {
        private const string ZipFileName = "BIG.zip";
        private const string XmlFileName = "BIG.xml";
        private const string ZipPassword = "mir123";

        private readonly IXmlConverter _converter = new XmlConverter();
        private readonly IXmlCalculate _calculator = new XmlCalculate();
        private readonly ITestOutputHelper _output;

        // xUnit инъецирует сюда ITestOutputHelper
        public XmlConverterTests(ITestOutputHelper output)
        {
            _output = output;
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

            // Конвертация
            var streams = _converter.Convert(bigStream).ToList();

            // Выводим итоговое количество файлов
            int totalFiles = streams.Count;
            int objectFiles = totalFiles - 1; // последний — это индексный файл
            _output.WriteLine($"Всего файлов после разделения: {totalFiles}");
            _output.WriteLine($"Файлов с объектами: {objectFiles}");
            _output.WriteLine($"Индексный файл: 1");

            // Проверка Calculate
            bigStream.Position = 0;
            var (origObjects, origProps) = _calculator.Calculate(bigStream);

            var linkStream = streams.Last();
            linkStream.Position = 0;
            var (linkObjects, linkProps) = _calculator.Calculate(linkStream);

            Assert.Equal(origObjects, linkObjects);
            Assert.Equal(origProps, linkProps);
        }

        [Fact]
        public void Convert_And_Calculate_MatchBigXmlSummary2()
        {
            var baseDir = AppContext.BaseDirectory;
            var bigFilePath = Path.Combine(baseDir, XmlFileName);

            Assert.True(File.Exists(bigFilePath),
                $"Не найден {XmlFileName} в {baseDir}");

            using var bigStream = File.OpenRead(bigFilePath);

            var streams = _converter.Convert(bigStream).ToList();

            bigStream.Position = 0;
            var (origObjects, origProps) = _calculator.Calculate(bigStream);

            var linkStream = streams.Last();
            linkStream.Position = 0;
            var (linkObjects, linkProps) = _calculator.Calculate(linkStream);

            Assert.Equal(origObjects, linkObjects);
            Assert.Equal(origProps, linkProps);
        }

        [Fact]
        public void Convert_NullStream_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _converter.Convert(null!));
        }

        [Fact]
        public void GetXmlFileType_BigXml_ReturnsBigXml()
        {
            var xml = "<Root><Objects><Object id='A' /></Objects></Root>";
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var doc = XDocument.Load(ms);
            var methodInfo = typeof(XmlConverter)
                             .GetMethod("GetXmlFileType",
                                        System.Reflection.BindingFlags.NonPublic |
                                        System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);
            var type = methodInfo!.Invoke(_converter, new object[] { doc });
            Assert.Equal(XmlFileType.BigXml, type);
        }

        [Fact]
        public void Convert_BigXml_ReturnsExpectedFileNamesAndIds()
        {
            var xml = @"
            <Root>
            <Objects>
                <Object id='1'/>
                <Object id='2'/>
            </Objects>
            </Root>";
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));

            var allStreams = _converter.Convert(ms).ToList();
            var objectStreams = allStreams.Take(allStreams.Count - 1).ToList();

            Assert.Equal(2, objectStreams.Count);

            var ids = objectStreams
                .Select(s =>
                {
                    s.Position = 0;
                    var doc = XDocument.Load(s);
                    var root = doc.Root ?? throw new InvalidDataException("Root element is missing.");
                    var idAttr = root.Attribute("id") ?? throw new InvalidDataException("Attribute 'id' is missing.");
                    return idAttr.Value;
                })
                .OrderBy(id => id)
                .ToList();

            Assert.Equal(new[] { "1", "2" }, ids);
        }

        [Fact]
        public void Convert_BigXml_OnlyFirstLevelObjectsToStreams_WithCorrectIds()
        {
            var xml = @"
            <Root>
            <Objects>
                <Object id='X'/>
                <Object id='Y'/>
            </Objects>
            </Root>";

            using var msIn = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var allStreams = _converter.Convert(msIn).ToList();
            var doc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
            var getType = typeof(XmlConverter)
                        .GetMethod("GetXmlFileType",
                                    BindingFlags.NonPublic | BindingFlags.Instance)!;
            var fileType = (XmlFileType)getType.Invoke(_converter, new object[] { doc })!;
            Assert.Equal(XmlFileType.BigXml, fileType);

            Assert.True(allStreams.Count >= 2, "Должно быть минимум 1 объект + 1 linkIndex");
            var objectStreams = allStreams.Take(allStreams.Count - 1).ToList();
            Assert.Equal(2, objectStreams.Count);

            var returnedIds = objectStreams
                .Select(s =>
                {
                    s.Position = 0;
                    var x = XDocument.Load(s).Root!;
                    return x.Attribute("id")!.Value;
                })
                .OrderBy(id => id)
                .ToList();

            Assert.Equal(new[] { "X", "Y" }, returnedIds);
        }


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

                var entry = zip.Entries
                    .FirstOrDefault(e =>
                        string.Equals(e.FileName, XmlFileName, StringComparison.OrdinalIgnoreCase));

                if (entry == null)
                    throw new InvalidDataException($"В архиве нет файла {XmlFileName}");

                entry.Extract(baseDir, ExtractExistingFileAction.OverwriteSilently);
            }
        }
    }
    public class XmlConverterDetectFileTypeTests
    {
        private readonly IXmlConverter _converter = new XmlConverter();

        [Fact]
        public void DetectFileType_Null_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _converter.DetectFileType(null!));
        }

        [Theory]
        [InlineData("<Root><Objects/></Root>", XmlFileType.BigXml)]
        [InlineData("<LinkXML format='LinkXML'/>", XmlFileType.LinkXml)]
        [InlineData("<LinkIndex format='LinkIndex'/>", XmlFileType.LinkIndex)]
        [InlineData("<AnythingElse/>", XmlFileType.Unknown)]
        public void DetectFileType_CommonXml_ReturnsExpected(string xml, XmlFileType expected)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));

            var result = _converter.DetectFileType(ms);

            Assert.Equal(expected, result);
        }
    }

}
