using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Windows.Forms;

using CommandLine;
using CommandLine.Text;

namespace IEsnapper
{
    class Program
    {
        static System.Windows.Forms.WebBrowser wb;
        static Timer pageLoadTimer;
        static Timer failsafeTimer;
        static Options options = new Options();

        class Options
        {
            [Option("w", "width", DefaultValue = 1920, HelpText = "Pixel width of browser")]
            public int Width { get; set; }
            [Option("h", "height", DefaultValue = 1080, HelpText = "Pixel height of browser")]
            public int Height { get; set; }
            
            [Option("u", "url", HelpText = "URL to load", Required = true)]
            public string Url { get; set; }

            [Option("o", "out", HelpText = "File to save to", Required = true)]
            public string OutFile { get; set; }

            [Option("f", "failsafe-delay", DefaultValue = 10000, HelpText = "Milliseconds to wait for some content before giving up")]
            public int FailsafeDelay { get; set; }
            [Option("l", "load-delay", DefaultValue = 5000, HelpText = "Milliseconds to wait after page load before snapshot")]
            public int LoadDelay { get; set; }

            [Option("v", "verbose", HelpText = "Verbose logging")]
            public bool Verbose { get; set; }

        }

        [HelpOption]
        public static string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("IEsnapper", "1.0"),
                Copyright = new CopyrightInfo("Andy Oakley", 2012),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine(" ");
            help.AddPreOptionsLine("Usage: IEsnapper -u http://www.bing.com -o bing.png");
            help.AddOptions(options);
            return help;
        }

        [STAThread]
        static void Main(string[] args)
        {
            ICommandLineParser parser = new CommandLineParser();
            if (!parser.ParseArguments(args, options))
            {
                Console.WriteLine(GetUsage());
                return;
            }

            SetupFailsafeTimer();
            SetupPageLoadTimer();
            SetupWebBrowser();

            if (options.Verbose)
            {
                Console.WriteLine("Capturing page at {0}x{1}.", options.Width, options.Height);
                Console.WriteLine("Requesting {0}.", options.Url);
            }

            wb.Navigate(options.Url);

            while (failsafeTimer.Enabled || pageLoadTimer.Enabled)
            {
                if (options.Verbose)
                {
                    Console.Write(".");
                }

                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }
        }

        private static void SetupWebBrowser()
        {
            wb = new System.Windows.Forms.WebBrowser();
            wb.Width = options.Width;
            wb.Height = options.Height;
            wb.ScriptErrorsSuppressed = true;
            wb.ScrollBarsEnabled = false;
            wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(wb_DocumentCompleted);
        }

        private static void SetupPageLoadTimer()
        {
            pageLoadTimer = new Timer();
            pageLoadTimer.Interval = options.LoadDelay;
            pageLoadTimer.Tick += new EventHandler(pageLoadTimer_Tick);
        }

        private static void SetupFailsafeTimer()
        {
            failsafeTimer = new Timer();
            failsafeTimer.Interval = options.FailsafeDelay;
            failsafeTimer.Tick += new EventHandler(failsafeTimer_Tick);
            failsafeTimer.Start();
        }

        static void failsafeTimer_Tick(object sender, EventArgs e)
        {
            failsafeTimer.Stop();
            Console.WriteLine("Failure: Failsafe timer expired before content was received");
        }



        static void wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // we at least got something back, will not exit through failsafe
            failsafeTimer.Stop();

            // reset the counter for every new DocumentCompleted event
            // we'll take the snapshot pageLoadTimer.Interval seconds after the last one is received
            if (pageLoadTimer.Enabled)
            {
                pageLoadTimer.Stop();
            }
            pageLoadTimer.Start();

            if (options.Verbose)
            {
                Console.Write("r");
            }
        }


        static void pageLoadTimer_Tick(object sender, EventArgs e)
        {
            pageLoadTimer.Stop();

            Rectangle rect = new Rectangle(0, 0, options.Width, options.Height);

            Bitmap bitmap = new Bitmap(rect.Width, rect.Height);
            Graphics g = Graphics.FromImage(bitmap);
            IntPtr hdc = g.GetHdc();
            IViewObject view = wb.Document.DomDocument as IViewObject;

            view.Draw(1, -1, (IntPtr)0, (IntPtr)0, (IntPtr)0, (IntPtr)hdc, ref rect, ref rect, (IntPtr)0, 0);

            g.ReleaseHdc(hdc);
            bitmap.Save(options.OutFile);
            bitmap.Dispose();

            if (options.Verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Saved to {0}", options.OutFile);
            }
        }


    }
}
