using BCrypt.Net;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter password: ");
        string password = Console.ReadLine()!;

        // Generate bcrypt hash with WorkFactor = 11 to match $2a$11$ pattern
        string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

        Console.WriteLine("\nGenerated Hash:");
        Console.WriteLine(hash);
    }
}
