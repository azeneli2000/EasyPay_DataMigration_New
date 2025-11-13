namespace Application.UnitTests
{
    using System.Collections.Generic;
    using Importer.Infrastructure.Matching;
    using Importer.Domain.Entities;
    using Xunit;

    public class LevenshteinFuzzyMatcherTests
    {
        private readonly LevenshteinFuzzyMatcher _sut = new();

        [Fact]
        public void Match_ReturnsExactClient_WhenNamesAreIdentical()
        {
            // Arrange
            var extracted = new Client(1, "Mario", "Rossi");
            var clients = new List<Client> { new Client(1, "Mario","Rossi")  };

            // Act
            var result = _sut.Match("Mario Rossi", clients);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(extracted.FullName, result.FullName);
        }

        [Fact]
        public void Match_ReturnsClosestMatch_WithinThreshold()
        {
            // Arrange
            var mario = new Client(1, "Mario" ,"Rossi");
            var clients = new List<Client>
        {
            new Client ( 1,"Luca", "Bianchi" ),
            mario
        };

            // Act
            var result = _sut.Match("Mario Rosi", clients); 

            // Assert
            Assert.Equal(mario, result);
        }

        [Fact]
        public void Match_ReturnsNull_WhenBestMatchIsTooDifferent()
        {
            // Arrange
            var clients = new List<Client>
        {
            new Client ( 1, "Mario", "Rossi" )
        };

            // Act
            var result = _sut.Match("ZZZZZZZZZ", clients);

            // Assert
            Assert.Null(result);
        }
    }

}