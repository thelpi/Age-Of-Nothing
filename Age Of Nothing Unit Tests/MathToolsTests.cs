using Age_Of_Nothing;
using Xunit;

namespace Age_Of_Nothing_Unit_Tests
{
    public class UnitTest1
    {
        [Theory]
        [InlineData(10, 10, 14, 13)]
        [InlineData(26, 22, 22, 19)]
        [InlineData(26, 10, 22, 13)]
        [InlineData(10, 22, 14, 19)]
        public void ComputePointOnLine_ExactPointOnLinearFunction_ShouldReturnExpected(double x1, double y1, double x2Expected, double y2Expected)
        {
            var xf = 18;
            var yf = 16;
            var distance = 5D;

            var (x2, y2) = GeometryTools.ComputePointOnLine(x1, y1, xf, yf, distance);

            Assert.Equal(x2Expected, x2);
            Assert.Equal(y2Expected, y2);
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData(26, 22)]
        [InlineData(26, 10)]
        [InlineData(10, 22)]
        public void ComputePointOnLine_BeyondTargetOnLinearFunction_ShouldReturnTarget(double x1, double y1)
        {
            var xf = 18;
            var yf = 16;
            var distance = 11D;

            var (x2, y2) = GeometryTools.ComputePointOnLine(x1, y1, xf, yf, distance);

            Assert.Equal(xf, x2);
            Assert.Equal(yf, y2);
        }

        [Fact]
        public void ComputePointOnLine_HorizontalLine_ShouldReturnExpected()
        {
            var distance = 20D;

            var (x2, y2) = GeometryTools.ComputePointOnLine(10, 10, 50, 10, distance);

            Assert.Equal(30, x2);
            Assert.Equal(10, y2);
        }

        [Fact]
        public void ComputePointOnLine_VerticalLine_ShouldReturnExpected()
        {
            var distance = 20D;

            var (x2, y2) = GeometryTools.ComputePointOnLine(10, 10, 10, 50, distance);

            Assert.Equal(10, x2);
            Assert.Equal(30, y2);
        }

        [Fact]
        public void ComputePointOnLine_BeyondTargetOnVerticalLine_ShouldReturnTarget()
        {
            var distance = 60D;

            var (x2, y2) = GeometryTools.ComputePointOnLine(10, 10, 10, 50, distance);

            Assert.Equal(10, x2);
            Assert.Equal(50, y2);
        }
    }
}
