using System.Collections.Generic;
using Xunit;
using Calculadora.Models;

namespace Calculadora.Tests
{
    public class StatisticsModelTests
    {
        [Fact]
        public void Mean_ReturnsCorrectAverage()
        {
            var data = new List<double> { 2, 4, 6, 8 };
            double mean = StatisticsModel.Mean(data);
            Assert.Equal(5.0, mean);
        }

        [Fact]
        public void LinearRegression_PerfectLine_ReturnsR2One()
        {
            // y = 2x + 1
            var points = new List<DataPoint>
            {
                new DataPoint(1, 3),
                new DataPoint(2, 5),
                new DataPoint(3, 7),
                new DataPoint(4, 9)
            };

            var res = StatisticsModel.LinearRegression(points);

            Assert.Equal(2.0, res.Slope, 4);
            Assert.Equal(1.0, res.Intercept, 4);
            Assert.Equal(1.0, res.R2, 4);
        }
    }
}
