

// Program.cs
using EnvelopeGenerator.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Envelope Generator Test");
        Console.WriteLine("--------------------------------");
        string error;

            TestRegularCurrent();


        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
    private static async Task TestRegularCurrent()
    {
        Console.WriteLine("\nTesting Regular Current File Generation...");

        var manager = new EnvelopeProcessManager(
            odbcName: "YoaGvia_Irit_160125",      // שם ה-ODBC להחליף בהתאם
            actionType: 1,                 // שוטף
            envelopeType: 100,              // סוג מעטפית
            batchNumber: 313, false, 20527);

        var result = await manager.GenerateEnvelopesAsync();
        if (result.Success)
        {
            Console.WriteLine("Regular current file generated successfully.");
        }
        else
        {
            Console.WriteLine($"Failed to generate regular current file: {result.ErrorMessage}");
        }
    }

    //private static void TestCombined()
    //{
    //    Console.WriteLine("\nTesting Combined File Generation...");

    //    var manager = new EnvelopeProcessManager(
    //        odbcName: "YourOdbcName",
    //        actionType: 3,                 // משולב
    //        envelopeType: 1,
    //        batchNumber: 202401
    //    );

    //    string error;
    //    if (manager.GenerateEnvelopes(out error))
    //    {
    //        Console.WriteLine("Combined files generated successfully.");
    //    }
    //    else
    //    {
    //        Console.WriteLine($"Failed to generate combined files: {error}");
    //    }
    //}
}