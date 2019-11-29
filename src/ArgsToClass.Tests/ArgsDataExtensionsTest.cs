using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace ArgsToClass.Tests
{
    using ArgsDataExtensionsTestSpace;

    namespace ArgsDataExtensionsTestSpace
    {
        public static class SelfExtensions
        {
            public static void Test(this Self self)
            {

            }
        }

        public class Self : IEnumerable<string>
        {
            public IEnumerator<string> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class Command1
        {
            public int CallCount { get; private set; }
            public void Run()
            {
                CallCount++;
            }

            public static int StaticCallCount { get; private set; }
            public static void Run(Command1 command)
            {
                StaticCallCount++;
            }
        }

        public static class Command1Extensions
        {
            public static int CallCount { get; private set; }
            public static void Run(this Command1 self)
            {
                CallCount++;
            }
        }

        public class Command2
        {
            public static int CallCount { get; private set; }
            public static void Run(Command2 command)
            {
                CallCount++;
            }
        }

        public class Command3
        {
            public static int CallCount { get; private set; }
            public static void Run(Command3 command)
            {
                CallCount++;
            }
        }

        public static class Command3Extensions
        {
            public static int CallCount { get; private set; }
            public static void Run(this Command3 self)
            {
                CallCount++;
            }
        }

        public class Command4
        {

        }
    }

    public class ArgsDataExtensionsTest
    {
        [Fact]
        public void GetExtensionMethodInfosTest()
        {
            var methodInfos = ArgsDataExtensions.GetExtensionMethodInfos(typeof(Self));
            Assert.Contains(typeof(SelfExtensions).GetMethod(nameof(SelfExtensions.Test)), methodInfos);
        }

        [Fact]
        public void RunCommandTest()
        {
            {
                var command = new Command1();
                ArgsDataExtensions.RunCommand(command);
                Assert.Equal(1, command.CallCount);
                Assert.Equal(0, Command1Extensions.CallCount);
                Assert.Equal(0, Command1.StaticCallCount);
            }
            {
                var command = new Command2();
                ArgsDataExtensions.RunCommand(command);
                Assert.Equal(1, Command2.CallCount);
            }
            {
                var command = new Command3();
                ArgsDataExtensions.RunCommand(command);
                Assert.Equal(1, Command3Extensions.CallCount);
            }
            {
                var command = new Command4();
                Assert.Throws<InvalidOperationException>(() => ArgsDataExtensions.RunCommand(command));
            }
        }
    }
}
