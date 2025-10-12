namespace Lab3.ProgramLoop
{
    internal class WrongAlphaValueException : Exception
    {
        public WrongAlphaValueException()
        {
            Console.WriteLine("Альфа должны быть >= 0 и их сумма равна 1");
        }
    }
}
