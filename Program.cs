using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using At.Matus.StatisticPod;
using Bev.Instruments.Msc15;

namespace CctMeter
{
    class Program
    {
        readonly static string fatSeparator = new string('=', 80);
        readonly static string thinSeparator = new string('-', 80);
        readonly static int maxIterations = 100;

        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            DateTime timeStamp = DateTime.UtcNow;
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            var appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            string appVersionString = $"{appVersion.Major}.{appVersion.Minor}";

            Options options = new Options();
            if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options))
                Console.WriteLine("*** ParseArgumentsStrict returned false");

            var streamWriter = new StreamWriter(options.LogFileName, true);
            var stpE = new StatisticPod("Statistics for illuminance");
            var stpCct = new StatisticPod("Statistics for cct");
            var stpT = new StatisticPod("Statistics for internal temperature");
            if (options.MaximumSamples >= maxIterations) options.MaximumSamples = maxIterations - 1;
            if (options.MaximumSamples < 2) options.MaximumSamples = 2;
            var device = new Msc15("MSC15_0");

            DisplayOnly("");
            LogOnly(fatSeparator);
            DisplayOnly($"Application:  {appName} {appVersionString}");
            LogOnly($"Application:  {appName} {appVersion}");
            LogAndDisplay($"StartTimeUTC: {timeStamp:dd-MM-yyyy HH:mm}");
            LogAndDisplay($"InstrumentID: {device.InstrumentManufacturer} {device.InstrumentID}");
            LogAndDisplay($"Samples (n):  {options.MaximumSamples}");
            LogAndDisplay($"Comment:      {options.UserComment}");
            LogOnly(fatSeparator);
            DisplayOnly("");

            MeasureDarkOffset();
            SetDynamicDarkMode();

            DisplayOnly("");
            DisplayOnly("press any key to start a measurement - 'd' to start with offset, 'q' to quit");

            int measurementIndex = 0;

            ConsoleKeyInfo cki;
            while ((cki=Console.ReadKey(true)).Key != ConsoleKey.Q)
            {
                if(cki.Key == ConsoleKey.D)
                {
                    MeasureDarkOffset();
                }
                int iterationIndex = 0;
                measurementIndex++;
                DisplayOnly("");
                DisplayOnly($"Measurement #{measurementIndex}");
                stpE.Restart();
                stpCct.Restart();
                stpT.Restart();
                timeStamp = DateTime.UtcNow;

                while (stpCct.SampleSize < options.MaximumSamples)
                {
                    iterationIndex++;
                    device.Measure();
                    stpE.Update(device.PhotopicValue);
                    stpCct.Update(device.CctValue);
                    stpT.Update(device.InternalTemperature);
                    DisplayOnly($"{stpCct.SampleSize,4}:   {device.CctValue:F0} K    {device.PhotopicValue:F2} lx");
                    if (iterationIndex >= maxIterations)
                    {
                        break;
                    }
                }
                if (iterationIndex >= maxIterations)
                {
                    DisplayOnly("To many iterations! Giving up ...");
                    break;
                }
                DisplayOnly("");
                LogOnly($"Measurement number:            {measurementIndex}");
                LogOnly($"Triggered at:                  {timeStamp:dd-MM-yyyy HH:mm:ss}");
                LogAndDisplay($"CCT value:                     {stpCct.AverageValue:F1} ± {stpCct.StandardDeviation:F1} K");
                LogAndDisplay($"Illuminance:                   {stpE.AverageValue:F2} ± {stpE.StandardDeviation:F2} lx");
                LogAndDisplay($"Internal temperature:          {stpT.AverageValue:F1} °C");
                LogOnly(thinSeparator);
                DisplayOnly("");
                DisplayOnly("press any key to start a measurement - 'd' to start with offset, 'q' to quit");
            }

            DisplayOnly("bye.");
            LogOnly("");
            LogOnly(fatSeparator);
            if (measurementIndex == 1)
                LogOnly($"{measurementIndex} measurement logged - StopTimeUTC: {timeStamp:dd-MM-yyyy HH:mm}");
            else
                LogOnly($"{measurementIndex} measurements logged - StopTimeUTC: {timeStamp:dd-MM-yyyy HH:mm}");
            LogOnly(fatSeparator);
            LogOnly("");

            streamWriter.Close();

            /***************************************************/
            void SetDynamicDarkMode()
            {
                if(device.HasShutter)
                {
                    device.ActivateDynamicDarkMode();
                    DisplayOnly("Dynamic dark mode activated.");
                }
                else
                {
                    device.DeactivateDynamicDarkMode();
                    DisplayOnly("Dynamic dark mode deactivated.");
                }
            }
            /***************************************************/
            void MeasureDarkOffset()
            {
                if (device.HasShutter)
                {
                    DisplayOnly("measure dark offset ...");
                    device.MeasureDark();
                }
                else
                {
                    // manual close shutter (only for MSC15)
                    DisplayOnly("close shutter and press enter");
                    Console.ReadLine();
                    device.MeasureDark();
                    DisplayOnly("open Shutter and press enter");
                    Console.ReadLine();
                }
            }
            /***************************************************/
            void LogAndDisplay(string line)
            {
                DisplayOnly(line);
                LogOnly(line);
            }
            /***************************************************/
            void LogOnly(string line)
            {
                streamWriter.WriteLine(line);
                streamWriter.Flush();
            }
            /***************************************************/
            void DisplayOnly(string line)
            {
                Console.WriteLine(line);
            }
            /***************************************************/

        }
    }
}
