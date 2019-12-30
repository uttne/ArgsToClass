using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ArgsToClass
{
    public class CommandSchemaTree:ReadOnlyDictionary<CommandSchema,IReadOnlyList<CommandSchema>>
    {
        internal CommandSchemaTree(IDictionary<CommandSchema, IReadOnlyList<CommandSchema>> dictionary) : base(dictionary)
        {
        }

        internal static CommandSchemaTree Create(IEnumerable<(CommandSchema parent,IEnumerable<CommandSchema> children)> commandTrees)
        {
            var dic = new Dictionary<CommandSchema,IReadOnlyList<CommandSchema>>();

            foreach (var commandTree in commandTrees.Where(x=>x.parent is null == false))
            {
                dic[commandTree.parent] = commandTree.children.ToArray();
            }

            return new CommandSchemaTree(dic);
        }

        public IReadOnlyList<SubCommandSchema> GetSubCommandSchemata(CommandSchema key)
        {
            return this[key].OfType<SubCommandSchema>().ToArray();
        }
    }
}