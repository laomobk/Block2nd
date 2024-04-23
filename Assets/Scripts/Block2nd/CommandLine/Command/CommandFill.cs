using System;
using Block2nd.Database;
using UnityEngine;

namespace Block2nd.CommandLine.Command
{
    public class CommandFill : ICommand
    {
        public CommandRuntimeError Execute(CommandRuntime rt, string[] args)
        {
            var playerPos = rt.GameClient.player.Position;

            if (args.Length != 8)
            {
                return new CommandRuntimeError(CommandRuntimeErrorTypes.SyntaxError,
                    "'fill' usage: fill [block_code] [x0 | ~] [y0 | ~] [z0 | ~] [x1 | ~] [y1 | ~] [z1 | ~]");
            }

            try
            {
                int blockCode;
                if (!int.TryParse(args[1], out blockCode))
                {
                    blockCode = BlockMetaDatabase.GetBlockCodeById(args[1]);
                    if (blockCode == 0)
                    {
                        return new CommandRuntimeError(
                            CommandRuntimeErrorTypes.RuntimeError,
                            "no suce block with code: " + blockCode);
                    }
                }
                
                int x0 = ConvertInt(args[2], playerPos.x);
                int y0 = ConvertInt(args[3], playerPos.y);
                int z0 = ConvertInt(args[4], playerPos.z);
                int x1 = ConvertInt(args[5], playerPos.x);
                int y1 = ConvertInt(args[6], playerPos.y);
                int z1 = ConvertInt(args[7], playerPos.z);

                int sx = Mathf.Min(x0, x1);
                int sy = Mathf.Min(y0, y1);
                int sz = Mathf.Min(z0, z1);
                
                int ex = Mathf.Max(x0, x1);
                int ey = Mathf.Max(y0, y1);
                int ez = Mathf.Max(z0, z1);

                var meta = BlockMetaDatabase.GetBlockMetaByCode(blockCode);
                if (meta == null && blockCode != 0)
                {
                    return new CommandRuntimeError(
                        CommandRuntimeErrorTypes.RuntimeError,
                        "no suce block with code: " + blockCode);
                }

                var level = rt.GameClient.CurrentLevel;

                for (int x = sx; x <= ex; ++x)
                {
                    for (int y = sy; y <= ey; ++y)
                    {
                        for (int z = sz; z <= ez; ++z)
                        {
                            level.SetBlock(blockCode, x, y, z);
                        }
                    }
                }

                return null;
            }
            catch (FormatException)
            { }
            
            return new CommandRuntimeError(
                CommandRuntimeErrorTypes.RuntimeError, "'fill' needs 6 float/int arguments");
        }

        private int ConvertInt(string arg, float defaultValue)
        {
            if (arg == "~")
            {
                return Mathf.FloorToInt(defaultValue);
            }

            return Mathf.FloorToInt(float.Parse(arg));
        }
    }
}