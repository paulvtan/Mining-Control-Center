using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiningControlCenter
{
    public class Helper
    {
        public static void Underline(string message, string symbol)
        {
            Console.WriteLine(string.Join("", Enumerable.Repeat(symbol, message.Length)));
        }

        public static void StartRefreshTimer(int secs)
        {
            //Console.Write("\nRefreshing in ");
            for (int s = secs; s >= 0; s--)
            {
                //Console.CursorLeft = 15;
                //Console.Write("{0} seconds.\n", s);    // Add space to make sure to override previous contents
                Thread.Sleep(1000);
            }
        }

        public static Configs GetConfig()
        {
            //Read config JSON array from config.txt
            string jsonString = System.IO.File.ReadAllText("config.txt");
            var JObject = Newtonsoft.Json.Linq.JObject.Parse(jsonString)["config"];
            //Each active config in the JSON deserialized the setting and assign to Configs object.
            Configs configs = new Configs();
            foreach (var config in JObject)
            {
                bool active = bool.Parse(config["active"].ToString());
                if (active)
                {
                    switch (config["platformName"].ToString())
                    {
                        //Each platform setting.
                        case "Dwarfpool":
                            DwarfPoolConfig dwarfPoolConfig = JsonConvert.DeserializeObject<DwarfPoolConfig>(config.ToString());
                            configs.DwarfPoolConfig = dwarfPoolConfig;
                            break;
                    }
                }

            }
            return configs;
        }



        public class Configs
        {
            public DwarfPoolConfig DwarfPoolConfig { get; set; }

        }

        public class DwarfPoolConfig
        {
            public bool Active { get; set; }
            public string PlatformName { get; set; }
            public string BaseUri { get; set; }
            public string WalletAddress { get; set; }
            public double PayOutLimit { get; set; }
        }



        public class ConsoleRectangle
        {
            private int hWidth;
            private int hHieght;
            private Point hLocation;
            private ConsoleColor hBorderColor;

            public ConsoleRectangle(int width, int hieght, Point location, ConsoleColor borderColor)
            {
                hWidth = width;
                hHieght = hieght;
                hLocation = location;
                hBorderColor = borderColor;
            }

            public Point Location
            {
                get { return hLocation; }
                set { hLocation = value; }
            }

            public int Width
            {
                get { return hWidth; }
                set { hWidth = value; }
            }

            public int Hieght
            {
                get { return hHieght; }
                set { hHieght = value; }
            }

            public ConsoleColor BorderColor
            {
                get { return hBorderColor; }
                set { hBorderColor = value; }
            }

            public void Draw()
            {
                string s = "╔";
                string space = "";
                string temp = "";
                for (int i = 0; i < Width; i++)
                {
                    space += " ";
                    s += "═";
                }

                for (int j = 0; j < Location.X; j++)
                    temp += " ";

                s += "╗" + "\n";

                for (int i = 0; i < Hieght; i++)
                    s += temp + "║" + space + "║" + "\n";

                s += temp + "╚";
                for (int i = 0; i < Width; i++)
                    s += "═";

                s += "╝" + "\n";

                Console.ForegroundColor = BorderColor;
                Console.CursorTop = hLocation.Y;
                Console.CursorLeft = hLocation.X;
                Console.Write(s);
                Console.ResetColor();
            }
        }

        public class ProgressBar : IDisposable, IProgress<double>
        {
            private const int blockCount = 50;
            private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
            private const string animation = @"|/-\";

            private readonly Timer timer;

            private double currentProgress = 0;
            private string currentText = string.Empty;
            private bool disposed = false;
            private int animationIndex = 0;

            public ProgressBar()
            {
                timer = new Timer(TimerHandler);

                // A progress bar is only for temporary display in a console window.
                // If the console output is redirected to a file, draw nothing.
                // Otherwise, we'll end up with a lot of garbage in the target file.
                if (!Console.IsOutputRedirected)
                {
                    ResetTimer();
                }
            }

            public void Report(double value)
            {
                // Make sure value is in [0..1] range
                value = Math.Max(0, Math.Min(1, value));
                Interlocked.Exchange(ref currentProgress, value);
            }

            private void TimerHandler(object state)
            {
                lock (timer)
                {
                    if (disposed) return;

                    int progressBlockCount = (int)(currentProgress * blockCount);
                    double percent = Math.Round((double)(currentProgress * 100), 3);
                    string text = string.Format(" |{0}{1}| {2,3}% {3}",
                        new string('█', progressBlockCount), new string('░', blockCount - progressBlockCount),
                        percent,
                        animation[animationIndex++ % animation.Length]);
                    UpdateText(text);

                    ResetTimer();
                }
            }

            private void UpdateText(string text)
            {
                // Get length of common portion
                int commonPrefixLength = 0;
                int commonLength = Math.Min(currentText.Length, text.Length);
                while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
                {
                    commonPrefixLength++;
                }

                // Backtrack to the first differing character
                StringBuilder outputBuilder = new StringBuilder();
                outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

                // Output new suffix
                outputBuilder.Append(text.Substring(commonPrefixLength));

                // If the new text is shorter than the old one: delete overlapping characters
                int overlapCount = currentText.Length - text.Length;
                if (overlapCount > 0)
                {
                    outputBuilder.Append(' ', overlapCount);
                    outputBuilder.Append('\b', overlapCount);
                }

                Console.Write(outputBuilder);
                currentText = text;
            }

            private void ResetTimer()
            {
                timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
            }

            public void Dispose()
            {
                lock (timer)
                {
                    disposed = true;
                    UpdateText(string.Empty);
                }
            }
        }
    }
}
