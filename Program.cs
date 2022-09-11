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
            var stpIntTime = new StatisticPod("integration time");
            if (options.MaximumSamples >= maxIterations) options.MaximumSamples = maxIterations - 1;
            if (options.MaximumSamples < 2) options.MaximumSamples = 2;
            var device = new Msc15(options.DeviceName);

            //  format user comment
            string prefixForIndex = $"{options.UserComment.Trim()} - ";
            if (string.IsNullOrWhiteSpace(options.UserComment))
            { 
                options.UserComment = "---";
                prefixForIndex = string.Empty;
            }

            DisplayOnly("");
            LogOnly(fatSeparator);
            DisplayOnly($"Application:  {appName} {appVersionString}");
            LogOnly($"Application:  {appName} {appVersion}");
            LogAndDisplay($"DLL version:  {device.DllVersion}");
            LogAndDisplay($"StartTimeUTC: {timeStamp:dd-MM-yyyy HH:mm}");
            LogAndDisplay($"InstrumentID: {device.InstrumentManufacturer} {device.InstrumentID}");
            LogAndDisplay($"Samples (n):  {options.MaximumSamples}");
            LogAndDisplay($"Comment:      {options.UserComment}");
            LogOnly(fatSeparator);
            DisplayOnly("");

            if (options.NoOffset == false) MeasureDarkOffset();
            SetDynamicDarkMode();

            DisplayOnly("");

            int measurementIndex = 0;

            bool shallLoop = true;
            while (shallLoop)
            {
                DisplayOnly("press any key to start a measurement - 'd' to get dark offset, 'q' to quit");
                ConsoleKeyInfo cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.Q:
                        shallLoop = false;
                        break;
                    case ConsoleKey.D:
                        MeasureDarkOffset();
                        LogOnly($"Dark offset measured at {DateTime.UtcNow:dd-MM-yyyy HH:mm:ss}");
                        LogOnly(thinSeparator);
                        break;
                    default:
                        int iterationIndex = 0;
                        measurementIndex++;
                        DisplayOnly("");
                        DisplayOnly($"Measurement #{measurementIndex}");
                        RestartValues();
                        timeStamp = DateTime.UtcNow;

                        while (iterationIndex < options.MaximumSamples)
                        {
                            iterationIndex++;
                            device.Measure();
                            UpdateValues();
                            DisplayOnly($"{stpCct.SampleSize,4}:   {device.CctValue:F0} K    {device.PhotopicValue:F2} lx");
                        }

                        DisplayOnly("");
                        LogOnly($"Measurement number:            {prefixForIndex}{measurementIndex}");
                        LogOnly($"Triggered at:                  {timeStamp:dd-MM-yyyy HH:mm:ss}");
                        LogAndDisplay($"CCT value:                     {stpCct.AverageValue:F1} ± {stpCct.StandardDeviation:F1} K");
                        LogAndDisplay($"Illuminance:                   {stpE.AverageValue:F3} ± {stpE.StandardDeviation:F3} lx");
                        LogAndDisplay($"Integration time:              {stpIntTime.AverageValue} s"); // the shortest time is 12 us
                        LogAndDisplay($"Internal temperature:          {stpT.AverageValue:F1} °C");
                        LogOnly(thinSeparator);
                        DisplayOnly("");
                        break;
                }
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
            void RestartValues()
            {
                stpE.Restart();
                stpCct.Restart();
                stpT.Restart();
                stpIntTime.Restart();
            }
            /***************************************************/
            void UpdateValues()
            {
                stpE.Update(device.PhotopicValue);
                stpCct.Update(device.CctValue);
                stpT.Update(device.InternalTemperature);
                stpIntTime.Update(device.GetLastIntegrationTime());
            }
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

        readonly static string fatSeparator = new string('=', 80);
        readonly static string thinSeparator = new string('-', 80);
        readonly static int maxIterations = 100;

    }
}
