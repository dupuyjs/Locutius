using CommandLine;
using System;

namespace Locutius.Capture
{
    public class Options
    {
        [Option('d', "debug", Required = false, HelpText = "Enable the debug mode (record audio files locally).")]
        public bool Debug { get; set; }

        [Option('u', "uri", Required = false, HelpText = "Uri of the audio gateway.")]
        public Uri Uri { get; set; }
    }
}
