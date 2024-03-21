namespace Block2nd.CommandLine
{
    public class CommandRuntimeErrorTypes
    {
        public static readonly string SyntaxError = "Syntax Error";
        public static readonly string RuntimeError = "Command Error";
    }
    
    public class CommandRuntimeError
    {
        public string errType;
        public string errMessage;

        public CommandRuntimeError(string errType, string errMessage)
        {
            this.errType = errType;
            this.errMessage = errMessage;
        }

        public override string ToString()
        {
            return "E[" + errType + ": " + errMessage + "]";
        }
    }
}