using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ArgsToClass
{
    public static class ArgsDataExtensions
    {
        internal static MethodInfo[] GetExtensionMethodInfos(Type type) =>
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => x.IsGenericType == false && x.IsSealed && x.IsNested == false)
                .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(x => x.IsDefined(typeof(ExtensionAttribute), false))
                // When self parameter is a generic type, the extension method cannot be obtained.
                .Where(x => x.GetParameters()[0].ParameterType.IsAssignableFrom(type))
                .ToArray();

        /// <summary>
        /// Command execution
        /// </summary>
        /// <typeparam name="TMainCommand"></typeparam>
        /// <param name="self"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void RunCommand<TMainCommand>(this IArgsData<TMainCommand> self)
                where TMainCommand : class, new()
        {
            var command = self.Command;
            if (command is null)
                throw new ArgumentException("Command property is null.");

            RunCommand(command);
        }

        internal static void RunCommand(object command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandType = command.GetType();

            var instanceRunMethodInfo = commandType.GetMethod(
                nameof(ICommand.Run),
                BindingFlags.Instance | BindingFlags.Public,
                null, new Type[0], null);
            if (instanceRunMethodInfo is null == false)
            {
                instanceRunMethodInfo.Invoke(command, null);
                return;
            }

            var extensionRunMethodInfo = GetExtensionMethodInfos(commandType)
                .FirstOrDefault(x =>
                    x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == commandType);
            if (extensionRunMethodInfo is null == false)
            {
                extensionRunMethodInfo.Invoke(null, new[] { command });
                return;
            }

            var staticRunMethodInfo = commandType.GetMethod(
                nameof(ICommand.Run),
                BindingFlags.Static | BindingFlags.Public,
                null, new[] {commandType}, null);
            if (staticRunMethodInfo is null == false)
            {
                staticRunMethodInfo.Invoke(null, new[] { command });
                return;
            }

            throw new InvalidOperationException(
                $"The '{nameof(ICommand.Run)}' method is not defined in the '{commandType.Name}'.");
        }
    }
}
