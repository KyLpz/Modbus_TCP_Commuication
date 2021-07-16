using System;
using System.Net;
using System.Threading.Tasks;

namespace App_Modbus_Communication
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello EVSE Controler !");
            bool ipIsValid = false;
            string ipInput = String.Empty;
            while (!ipIsValid)
            {
                Console.WriteLine("Please Set the EVCC IP");
                ipInput = Console.ReadLine();
                try
                {
                    Utils.ValidateIpAddress(ipInput);
                    ipIsValid = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            var communication = new EvChargeControlPxcAdvanced(IPAddress.Parse(ipInput), 180, 502);
            try
            {
                var evccStatus = await communication.GetEvStatusAsync();
                Console.WriteLine($"Acutal - Status EVSE: {evccStatus.Item1} | Current Setting: {evccStatus.Item2} ");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");

                Console.ResetColor();
                Console.ReadLine();
                return;
            }
          

           

            Console.WriteLine($"Set enable charge (true or false):");
            var inputEnableCharge = Console.ReadLine();
            if (inputEnableCharge == bool.TrueString)
            {
                await communication.SetEnableChargingProcess(true);
            }
            else
            {
                await communication.SetEnableChargingProcess(false);
            }
            Console.WriteLine($"Set new charging currunt(0 -> abort):");
            int inputChargingCurrent = int.Parse(Console.ReadLine());
            if (inputChargingCurrent != 0)
            {
                await communication.SetChargeCurrentAsync(inputChargingCurrent);
            }

            Console.WriteLine($"Show live status? (y -> do it):");
            string inputShowStatus = Console.ReadLine();
            if (inputShowStatus == "y")
            {
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        var evccStatus = await communication.GetEvStatusAsync();
                        Console.WriteLine($"{DateTime.Now} - Status EVSE: {evccStatus.Item1} | Current Setting: {evccStatus.Item2} ");
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }

                });
            }
            Console.WriteLine($"Press Enter to exit. ");
            Console.ReadLine();

        }
    }
}
