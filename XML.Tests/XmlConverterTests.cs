using System.IO;
using System.Linq;
using System.Text;

using XML.lib;

using Xunit;

namespace MyXmlProject.Tests
{
    public class XmlConverterTests
    {
        private const string SampleBigXml = @"
        <BigXML>
            <Objects>
                <Object id='1'>
                    <Property name='Name'>Object1</Property>
                    <Property name='Value'>100</Property>
                </Object>
                <Object id='2'>
                    <Property name='Name'>Object2</Property>
                    <Property name='Value'>200</Property>
                </Object>
            </Objects>
            <Links>
                <Link from='1' to='2'/>
            </Links>
        </BigXML>";

        private readonly IXmlConverter _converter;
        private readonly IXmlCalculator _calculator;

        public XmlConverterTests()
        {
            _converter = new XmlConverter();
            _calculator = new XmlCalculator();
        }

        [Fact]
        public void Convert_And_Calculate_Results_Should_Match()
        {
            // Создаем поток из строки напрямую, чтобы избежать закрытия потока
            var bigXmlBytes = Encoding.UTF8.GetBytes(SampleBigXml);
            using (var bigXmlStream = new MemoryStream(bigXmlBytes))
            {
                // Получаем потоки после конвертации
                var resultStreams = _converter.Convert(bigXmlStream).ToList();

                // Пересчитываем данные из оригинального BigXML
                bigXmlStream.Position = 0;
                var (bigXmlObjects, bigXmlProperties) = _calculator.Calculate(bigXmlStream);

                // Предполагаем, что LinkXML – последний поток в списке
                var linkXmlStream = resultStreams.Last();
                linkXmlStream.Position = 0;
                var (linkXmlObjects, linkXmlProperties) = _calculator.Calculate(linkXmlStream);

                // Сравниваем результаты: рассчитанные количества объектов и свойств должны совпадать
                Assert.Equal(bigXmlObjects, linkXmlObjects);
                Assert.Equal(bigXmlProperties, linkXmlProperties);
            }
        }
    }
}
