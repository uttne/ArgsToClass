using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ArgsAnalyzer.Tests
{
    public class ArgsDataTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ArgsDataTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public class Option
        {

        }


        [Fact]
        public void Has_test()
        {
        }
    }
}
