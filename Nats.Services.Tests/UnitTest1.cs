using NFluent;
using System;
using Xunit;

namespace Nats.Services.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Check.That(1+1).IsEqualTo(2);
        }
    }
}
