class MyCalculator
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\nTo exit enter 'q'\n");
            bool ext;

            double num1;
            (num1, ext) = ReadNumber("Enter first number: ");
            if (ext)
            {
                break;
            }

            string op;
            (op, ext) = ReadOperation("Enter operation (+, -, *, /): ");
            if (ext)
            {
                break;
            }

            double num2;
            (num2, ext) = ReadNumber("Enter second number: ");
            if (ext)
            {
                break;
            }

            double result;
            switch (op)
            {
                case "+":
                    result = num1 + num2;
                    break;

                case "-":
                    result = num1 - num2;
                    break;

                case "*":
                    result = num1 * num2;
                    break;

                case "/":
                    if (num2 == 0)
                    {
                        Console.WriteLine("Error: Division by zero is not allowed.");
                        continue;
                    }

                    result = num1 / num2;
                    break;

                default:
                    Console.WriteLine("Invalid operation. Please use +, -, *, or /.");
                    continue;
            }

            Console.WriteLine($"Result: {num1} {op} {num2} = {result}\n");
        }
    }

    const string QuitSymbol = "q";
    const string Operations = "+-*/";

    static (double, bool) ReadNumber(string msg)
    {
        while (true)
        {
            Console.Write(msg);

            string line = Console.ReadLine() ?? "";
            if (line == QuitSymbol)
            {
                return (0.0, true);
            }

            if (double.TryParse(line, out double result))
            {
                return (result, false);
            }

            Console.WriteLine("Invalid number. Try again.");
        }
    }

    static (string, bool) ReadOperation(string msg)
    {
        while (true)
        {
            Console.Write(msg);

            string line = Console.ReadLine() ?? "";
            if (line == QuitSymbol)
            {
                return ("", true);
            }

            if (line.Length == 1 && Operations.Contains(line))
            {
                return (line, false);
            }

            Console.WriteLine("Invalid operation. Try again.");
        }
    }
}
