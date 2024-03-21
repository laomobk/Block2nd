namespace Block2nd.CommandLine.Command
{
    public interface ICommand
    {
        CommandRuntimeError Execute(CommandRuntime rt, string[] args);
    }
}