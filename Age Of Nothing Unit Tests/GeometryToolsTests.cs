using Age_Of_Nothing;
using Xunit;

namespace Age_Of_Nothing_Unit_Tests
{
    public class GeometryToolsTests
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

            var pt = GeometryTools.ComputePointOnLine(x1, y1, xf, yf, distance);

            Assert.Equal(x2Expected, pt.X);
            Assert.Equal(y2Expected, pt.Y);
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

            var pt = GeometryTools.ComputePointOnLine(x1, y1, xf, yf, distance);

            Assert.Equal(xf, pt.X);
            Assert.Equal(yf, pt.Y);
        }

        [Fact]
        public void ComputePointOnLine_HorizontalLine_ShouldReturnExpected()
        {
            var distance = 20D;

            var pt = GeometryTools.ComputePointOnLine(10, 10, 50, 10, distance);

            Assert.Equal(30, pt.X);
            Assert.Equal(10, pt.Y);
        }

        [Fact]
        public void ComputePointOnLine_VerticalLine_ShouldReturnExpected()
        {
            var distance = 20D;

            var pt = GeometryTools.ComputePointOnLine(10, 10, 10, 50, distance);

            Assert.Equal(10, pt.X);
            Assert.Equal(30, pt.Y);
        }

        [Fact]
        public void ComputePointOnLine_BeyondTargetOnVerticalLine_ShouldReturnTarget()
        {
            var distance = 60D;

            var pt = GeometryTools.ComputePointOnLine(10, 10, 10, 50, distance);

            Assert.Equal(10, pt.X);
            Assert.Equal(50, pt.Y);
        }
    }
}
