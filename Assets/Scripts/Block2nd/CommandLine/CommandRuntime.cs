using Block2nd.Client;
using Block2nd.CommandLine.Command;

namespace Block2nd.CommandLine
{
    public class CommandRuntime
    {
        public GameClient GameClient { get; }

        public CommandRuntime(GameClient gameClient)
        {
            this.GameClient = gameClient;
        }

        /// <summary>
        ///     execute command in raw chat string, like "/tp 7 2 7"
        /// </summary>
        /// <param name="rawCommand">raw command string</param>
        public CommandRuntimeError ExecuteCommandRaw(string rawCommand)
        {
            if (!rawCommand.StartsWith("/"))
                return new CommandRuntimeError("Syntax Error", "not a command.");

            var cmd = rawCommand.Substring(1);
            var parts = cmd.Split(' ');

            if (parts.Length == 0)
            {
                return new CommandRuntimeError("Syntax Error", "not a command.");
            }
            
            return ExecuteCommand(parts[0], parts);
        }

        public CommandRuntimeError ExecuteCommand(string prefix, string[] args)
        {
            var cmd = CommandProvider.Get(prefix);

            if (cmd == null)
            {
                return new CommandRuntimeError("Command Error", "command not found.");
            }

            return cmd.Execute(this, args);
        }
    }
}