using System;
using Block2nd.Utils;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.CommandLine.Command
{
    public class CommandTime : ICommand
    {
        private string helpString = "/time [set] [args...]";
        
        public CommandRuntimeError Execute(CommandRuntime rt, string[] args)
        {
            int state = 0;
            
            Debug.Log(ArrayFormat.Format(args));

            foreach (var arg in args)
            {
                if (arg == "set")
                {
                    state = 1;
                } else if (arg == "time") 
                    continue;

                switch (state)
                {
                    case 0:
                    {
                        goto err;
                    }
                    case 1:
                    {
                        if (int.TryParse(arg, out int timeNum))
                        {
                            if (rt.GameClient.TryGetCurrentLevel(out Level level))
                            {
                                level.levelTime = timeNum;
                            }

                            return null;
                        }
                        break;
                    }
                }
            }
            
            err:
            return new CommandRuntimeError(
                CommandRuntimeErrorTypes.SyntaxError, helpString);
        }
    }
}