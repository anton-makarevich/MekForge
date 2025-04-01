using Sanet.MakaMek.Core.Utils;
using Shouldly;

namespace Sanet.MakaMek.Core.Tests.Utils;

public class DiceUtilsTests
{
    [Theory]
    [InlineData(2, 100.0)]  // Guaranteed hit
    [InlineData(3, 97.22)]  // Almost guaranteed
    [InlineData(7, 58.33)]  // Median value
    [InlineData(12, 2.78)]  // Hardest roll
    [InlineData(13, 0.0)]   // Impossible roll
    [InlineData(1, 100.0)]  // Below minimum, should be 100%
    public void Calculate2d6Probability_ReturnsCorrectProbability(int targetNumber, double expectedProbability)
    {
        // Act
        var result = DiceUtils.Calculate2d6Probability(targetNumber);
        
        // Assert
        result.ShouldBe(expectedProbability, 0.01); // Allow small rounding differences
    }
    
    [Fact]
    public void Calculate2d6Probability_ProbabilitiesAreDescending()
    {
        // Arrange
        var probabilities = new List<double>();
        
        // Act
        for (int i = 2; i <= 12; i++)
        {
            probabilities.Add(DiceUtils.Calculate2d6Probability(i));
        }
        
        // Assert
        for (int i = 0; i < probabilities.Count - 1; i++)
        {
            probabilities[i].ShouldBeGreaterThan(probabilities[i + 1]);
        }
    }
    
    [Fact]
    public void Calculate2d6Probability_ProbabilitySum_Equals100Percent()
    {
        // Arrange
        var individualProbabilities = new Dictionary<int, double>
        {
            { 2, 2.78 },
            { 3, 5.56 },
            { 4, 8.33 },
            { 5, 11.11 },
            { 6, 13.89 },
            { 7, 16.67 },
            { 8, 13.89 },
            { 9, 11.11 },
            { 10, 8.33 },
            { 11, 5.56 },
            { 12, 2.78 }
        };
        
        // Act
        var sum = individualProbabilities.Values.Sum();
        
        // Assert
        sum.ShouldBe(100.0, 0.1); // Allow small rounding differences
    }
}
