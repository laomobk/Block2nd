namespace Block2nd.CommandLine.Command
{
    public class CommandFly: ICommand
    {
        public CommandRuntimeError Execute(CommandRuntime rt, string[] args)
        {
            rt.GameClient.Player.playerController.flying = true;

            return null;
        }
    }
}