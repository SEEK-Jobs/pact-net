using PactNet.Infrastructure.Outputters;
using Xunit;

namespace PactNet.Tests.PactMessage
{
    public class OutputBuilderTests
    {
        [Fact]
        public void WriteLine_WhenCalledWithNullValue_NoExceptionIsThrown()
        {
            //Arrange
            var outputBuilder = new OutputBuilder();

            //Act + Assert
            outputBuilder.WriteLine(null);
        }

        [Fact]
        public void WriteLine_WhenCalledWithContent_AppendsTheContentInNewLine()
        {
            //Arrange
            var outputBuilder = new OutputBuilder();
            var firstMessage = "test 1";
            var secondMessage = "test 2";

            var expectedMessage = "test 1\r\ntest 2\r\n";

            outputBuilder.WriteLine(firstMessage);

            //Act
            outputBuilder.WriteLine(secondMessage);

            //Assert
            Assert.Equal(expectedMessage, outputBuilder.ToString());
        }


        [Fact]
        public void Clear_WriteLineWanNotCalledFirst_NoExceptionIsThrown()
        {
            //Arrange
            var outputBuilder = new OutputBuilder();

            //Act + Assert
            outputBuilder.Clear();
        }

        [Fact]
        public void Clear_WhenWriteLineWasCalledFirst_DeletesTheContent()
        {
            //Arrange
            var outputBuilder = new OutputBuilder();

            outputBuilder.WriteLine("test");

            //Act
            outputBuilder.Clear();

            //Assert
            Assert.Equal(string.Empty, outputBuilder.ToString());
        }
    }
}
