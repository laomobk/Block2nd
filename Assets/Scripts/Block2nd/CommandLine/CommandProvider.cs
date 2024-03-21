using System.Collections.Generic;
using Block2nd.CommandLine.Command;

namespace Block2nd.CommandLine
{
    public class CommandProvider
    {
        private static readonly Dictionary<string, ICommand> commandDict = new Dictionary<string, ICommand>();

        static CommandProvider()
        {
            commandDict.Add("tp", new CommandTeleport());
            commandDict.Add("fly", new CommandFly());
        }

        public static ICommand Get(string prefix)
        {
            if (commandDict.TryGetValue(prefix, out var command))
            {
                return command;
            }

            return null;
        }

        public static void Add(string prefix, ICommand command)
        {
            commandDict.Add(prefix, command);
        }
    }
}