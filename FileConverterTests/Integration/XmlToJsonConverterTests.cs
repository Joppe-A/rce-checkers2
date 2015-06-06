using System.IO;
using FileConverter;
using NUnit.Framework;

namespace FileConverterTests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class XmlToJsonConverterTests
    {
        private XmlToJsonConverter _converter;
        private string _targetFilePathName;

        [SetUp]
        public void Setup()
        {
            _converter = new XmlToJsonConverter();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_targetFilePathName)) File.Delete(_targetFilePathName);
        }

        [Test]
        public void Convert_should_write_a_converted_file()
        {
            // arrange
            string sourceFile = Path.Combine(TestHelpers.GetTestFilesDir(), "TikaTest.docx.xhtml");
            
            _targetFilePathName = sourceFile + ".json";

            // act
            _converter.Convert(sourceFile, _targetFilePathName);

            // assert
            Assert.IsTrue(File.Exists(_targetFilePathName), "Converted file doesn't exist!");
        }

        [Test]
        [ExpectedException(typeof(FileDoesNotExistException))]
        public void Convert_with_non_existing_source_file_should_throw()
        {
            // arrange

            // act
            _converter.Convert("nonexistingfile", "sometargetpath");

            // assert

        }
        
    }
}
