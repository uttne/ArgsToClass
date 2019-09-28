using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ArgsToClass.Tests
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
            public string Text1 { get; set; }
            public string Text2 { get; set; }
            public Command Command { get; set; }
        }

        public class Command
        {
            public string Help { get; set; }
        }


        [Fact]
        public void Has_test()
        {
            var type = typeof(ArgsParser<>)
                .Assembly
                .GetType("ArgsAnalyzer.ArgsData`1")
                .MakeGenericType(typeof(Option));

            var option = new Option();
            var expressionTextHashSet = new HashSet<string>()
            {
                ".Text1",
                ".Command.Help",
            };
            var extra = new string[0];
            var target = Activator.CreateInstance(type, option, expressionTextHashSet, extra);

            {
                var method = target.GetType().GetMethod("Has")
                    .MakeGenericMethod(typeof(string));

                var expression = (Expression<Func<Option, string>>)(x => x.Text1);

                var actual = method.Invoke(target, new object[] { expression });
                Assert.True((bool) actual);
            }

            {
                var method = target.GetType().GetMethod("Has")
                    .MakeGenericMethod(typeof(string));

                var expression = (Expression<Func<Option, string>>)(x => x.Text2);

                var actual = method.Invoke(target, new object[] { expression });
                Assert.False((bool)actual);
            }

            {
                var method = target.GetType().GetMethod("Has")
                    .MakeGenericMethod(typeof(string));

                var expression = (Expression<Func<Option, string>>) (x => x.Command.Help);

                var actual = method.Invoke(target, new object[] {expression});
                Assert.True((bool)actual);
            }

            {
                var method = target.GetType().GetMethod("Has")
                    .MakeGenericMethod(typeof(Option));

                var expression = (Expression<Func<Option, Option>>)(x => x);

                var actual = method.Invoke(target, new object[] { expression });
                Assert.False((bool)actual);
            }

        }
    }
}
