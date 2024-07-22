using System;
using UnityEngine;

namespace Block2nd.CommandLine.Command
{
    public class CommandTeleport : ICommand
    {
        public CommandRuntimeError Execute(CommandRuntime rt, string[] args)
        {
            if (args.Length < 4)
            {
                return new CommandRuntimeError(
                    CommandRuntimeErrorTypes.RuntimeError, "'tp' needs 3 arguments");
            }

            try
            {
                float x = Single.Parse(args[1]);
                float y = Single.Parse(args[2]);
                float z = Single.Parse(args[3]);
            
                rt.GameClient.Player.ResetPlayer(new Vector3(x, y, z));

                return null;
            }
            catch (FormatException)
            {
                return new CommandRuntimeError(
                    CommandRuntimeErrorTypes.RuntimeError, "'tp' needs 3 float/int arguments");
            }
        }
    }
}