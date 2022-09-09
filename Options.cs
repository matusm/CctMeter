using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace CctMeter
{
    public class Options
    {
        [Option('n', "number", DefaultValue = 10, HelpText = "Number of samples.")]
        public int MaximumSamples { get; set; }

        [Option("comment", DefaultValue = "", HelpText = "User supplied comment string.")]
        public string UserComment { get; set; }

        [Option("device", DefaultValue = "MSC15_0", HelpText = "Device name.")]
        public string DeviceName { get; set; }

        [Option("logfile", DefaultValue = "cctmeter.log", HelpText = "Log file path.")]
        public string LogFileName { get; set; }

        [Option('s', "skipdark", DefaultValue = false, HelpText = "Skip dark offset measurement at startup.")]
        public bool NoOffset { get; set; }

        //[ValueList(typeof(List<string>), MaximumElements = 2)]
        //public IList<string> ListOfFileNames { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string AppVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            HelpText help = new HelpText
            {
                Heading = new HeadingInfo($"{AppName}, version {AppVer}"),
                Copyright = new CopyrightInfo("Michael Matus", 2021),
                AdditionalNewLineAfterOption = false,
                AddDashesToOption = true
            };
            string preamble = "Program to operate a spectroradiometers by Gigahertz-Optik. It is controlled via its USB interface. " +
                "Measurement results are logged in a file.";
            help.AddPreOptionsLine(preamble);
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine($"Usage: {AppName} [options]");
            help.AddPostOptionsLine("");
            help.AddOptions(this);

            return help;
        }
    }
}
