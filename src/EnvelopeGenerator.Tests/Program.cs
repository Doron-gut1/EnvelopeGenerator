using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvelopeGenerator.Core;

namespace EnvelopeGenerator.Tests
{

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting Envelope Generator Test");
            Console.WriteLine("--------------------------------");

            try
            {

                var manager = new EnvelopeProcessManager(
                odbcName: "betgvia",      // שם ה-ODBC להחליף בהתאם
                actionType: 1,                 // שוטף
                envelopeType: 1,              // סוג מעטפית
                batchNumber: 315           // מספר מנה
            );

                string error;
                if (manager.GenerateEnvelopes(out error))
                {
                    Console.WriteLine("Regular current file generated successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to generate regular current file: {error}");
                }


                // הפעלת בדיקה לקובץ שוטף רגיל
                TestRegularCurrent();

                // הפעלת בדיקה לקובץ שוטף + שנתי
                TestCurrentWithYearly();

                // הפעלת בדיקה לקובץ חוב
                TestDebt();

                // הפעלת בדיקה לקובץ משולב
                TestCombined();

                Console.WriteLine("\nAll tests completed. Check output files.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static void TestRegularCurrent()
        {
            Console.WriteLine("\nTesting Regular Current File Generation...");

            var manager = new EnvelopeProcessManager(
                odbcName: "YourOdbcName",      // שם ה-ODBC להחליף בהתאם
                actionType: 1,                 // שוטף
                envelopeType: 1,              // סוג מעטפית
                batchNumber: 202401           // מספר מנה
            );

            string error;
            if (manager.GenerateEnvelopes(out error))
            {
                Console.WriteLine("Regular current file generated successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to generate regular current file: {error}");
            }
        }

        private static void TestCurrentWithYearly()
        {
            Console.WriteLine("\nTesting Current + Yearly File Generation...");

            var manager = new EnvelopeProcessManager(
                odbcName: "YourOdbcName",
                actionType: 1,
                envelopeType: 1,
                batchNumber: 202401,
                isYearly: true                // עם שנתי
            );

            string error;
            if (manager.GenerateEnvelopes(out error))
            {
                Console.WriteLine("Current + Yearly file generated successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to generate Current + Yearly file: {error}");
            }
        }

        private static void TestDebt()
        {
            Console.WriteLine("\nTesting Debt File Generation...");

            var manager = new EnvelopeProcessManager(
                odbcName: "YourOdbcName",
                actionType: 2,                 // חוב
                envelopeType: 1,
                batchNumber: 202401
            );

            string error;
            if (manager.GenerateEnvelopes(out error))
            {
                Console.WriteLine("Debt file generated successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to generate debt file: {error}");
            }
        }

        private static void TestCombined()
        {
            Console.WriteLine("\nTesting Combined File Generation...");

            var manager = new EnvelopeProcessManager(
                odbcName: "YourOdbcName",
                actionType: 3,                 // משולב
                envelopeType: 1,
                batchNumber: 202401
            );

            string error;
            if (manager.GenerateEnvelopes(out error))
            {
                Console.WriteLine("Combined files generated successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to generate combined files: {error}");
            }
        }
    }

}
