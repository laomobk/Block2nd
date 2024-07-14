namespace Block2nd.Utils
{
    public static class ArrayFormat
    {
        public static string Format<T>(T[] array)
        {
            string result = "[";

            for (int i = 0; i < array.Length; ++i)
            {
                if (i > 0)
                {
                    result += ", ";
                }
                result += array[i].ToString();
            }

            return result + "]";
        }
    }
}