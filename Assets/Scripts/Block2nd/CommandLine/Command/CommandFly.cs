namespace Block2nd.CommandLine.Command
{
    public class CommandFly: ICommand
    {
        public CommandRuntimeError Execute(CommandRuntime rt, string[] args)
        {
            rt.GameClient.player.playerController.flying = true;

            return null;
        }
    }
}