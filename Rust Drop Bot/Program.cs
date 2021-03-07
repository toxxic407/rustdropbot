using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections;

namespace Rust_Drop_Bot
{
    class Program
    {
        //disable sleeping
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }
        static void Main()
        {
            if (!File.Exists("path.txt"))
            {
                Console.Write("Please enter the Path to your Chrome Install Location (i.e. C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe):  ");
                File.WriteAllText("path.txt", Console.ReadLine());
            }
            string path = File.ReadAllText("path.txt");
            Console.WriteLine("Getting List of Streamers with active Drops");
            Console.WriteLine("");
            String status_website = Get("https://twitch.facepunch.com/");
            String general_drops = status_website.Substring(status_website.IndexOf("general-drops"));
            status_website = status_website.Substring(0, status_website.IndexOf("general-drops"));
            Streamer[] stats = new Streamer[CountStringOccurrences(status_website, "streamer-name")];
            if (!File.Exists("stats.json"))
            {
                stats = Get_status(status_website, stats.Length);
                Save_progress(stats);
            }
            else
            {
                Streamer[] old_stats = update_stats();
                bool old = false;
                if (old_stats.Length == stats.Length)
                {
                    stats = Get_status(status_website, stats.Length);
                    for(int i = 0; i < stats.Length; i++)
                    {
                        if(stats[i].Name != old_stats[i].Name)
                        {
                            old = true;
                        }
                    }
                }
                else
                {
                    old = true;
                }

                if(old == true)
                {
                    Console.WriteLine("Updating stats.json with new Round of Drops");
                    Console.WriteLine("");
                    stats = Get_status(status_website, stats.Length);
                    Save_progress(stats);
                }
                else
                {
                    stats = old_stats;
                }
            }

            for (int i = 0; i < stats.Length; i++)
            {
                Console.WriteLine("Name: " + stats[i].Name + " URL: " + stats[i].URL);
            }
            Console.WriteLine("");

            Console.WriteLine("Getting List of Streamers with general Drops");
            Console.WriteLine("");
            Streamer[] general_stats = new Streamer[CountStringOccurrences(general_drops, "streamer-name")];
            general_stats = Get_status(general_drops, general_stats.Length);
            for (int i = 0; i < general_stats.Length; i++)
            {
                Console.WriteLine("Name: " + general_stats[i].Name + " URL: " + general_stats[i].URL);
            }
            Console.WriteLine("");

            while (true)
            {
                Console.WriteLine("Finding an unclaimed Streamer who is live right now");
                Console.WriteLine("");

                int Current_Stream;
                while (true)
                {
                    bool retry;
                    do
                    {
                        try
                        {
                            status_website = Get("https://twitch.facepunch.com/");
                            retry = false;
                        }
                        catch (Exception e)
                        {
                            retry = true;
                            Console.WriteLine("Could not reach twitch.facepunch.com to pull StreamerData (" + e + ") trying again in one Minute");
                            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                            Thread.Sleep(60000);
                        }
                    } while (retry == true);
                    status_website = status_website.Substring(0, status_website.IndexOf("general-drops"));
                    stats = update_stats();
                    stats = OnlineFinder(stats, status_website);
                    Current_Stream = 1337;
                    int highest_Priority = 0;
                    for (int i = 0; i < stats.Length; i++)
                    {
                        if (stats[i].online == true && (stats[i].Priority > highest_Priority || highest_Priority == 0))
                        {
                            highest_Priority = stats[i].Priority;
                            Current_Stream = i;
                        }
                    }
                    if (Current_Stream == 1337)
                    {
                        Console.WriteLine("Nobody with unclaimed drops is online. Retrying in 1 Minute");
                        Console.WriteLine("");
                        SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                        Thread.Sleep(60000);
                    }
                    else
                    {
                        break;
                    }
                }
                Console.WriteLine("Opening Chrome to watch " + stats[Current_Stream].Name);
                Console.WriteLine("");

                Process StreamWindow = Process.Start(path, stats[Current_Stream].URL);
                stats = update_stats();
                Console.WriteLine(stats[Current_Stream].Watchtime + "/130 Minutes (" + stats[Current_Stream].Name + ")");
                for (int i = stats[Current_Stream].Watchtime; i < 130; i++)
                {
                    SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                    Thread.Sleep(60000);
                    try
                    {
                        status_website = Get("https://twitch.facepunch.com/");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Could not reach twitch.facepunch.com to update StreamerData (" + e + ") trying again in one Minute");
                    }
                    status_website = status_website.Substring(0, status_website.IndexOf("general-drops"));

                    stats = update_stats();
                    Streamer[] new_stats = OnlineFinder(stats, status_website);
                    bool has_priority = true;
                    for (int j = 0; j < stats.Length; j++)
                    {
                        if (new_stats[j].Priority > stats[Current_Stream].Priority && new_stats[j].online == true)
                        {
                            has_priority = false;
                        }
                    }

                    if (has_priority != true)
                    {
                        Console.WriteLine("A Streamer with a higher Priority is now online");
                        Console.WriteLine("");
                        break;
                    }

                    if (new_stats[Current_Stream].online == true)
                    {
                        stats[Current_Stream].Watchtime++;
                        if(stats[Current_Stream].Completed == true)
                        {
                            break;
                        }
                        Save_progress(stats);
                        Console.WriteLine(stats[Current_Stream].Watchtime + "/130 Minutes (" + stats[Current_Stream].Name + ")");
                    }
                    else
                    {
                        Console.WriteLine(stats[Current_Stream].Name + " is no longer online");
                        Console.WriteLine("");
                        break;
                    }
                }
                if (stats[Current_Stream].Watchtime >= 130 || stats[Current_Stream].Completed == true)
                {
                    stats[Current_Stream].Completed = true;
                    Save_progress(stats);
                    Console.WriteLine("Required Watchtime completed (" + stats[Current_Stream].Name + ")");
                    Console.WriteLine("");
                }
                try
                {
                    StreamWindow.Kill();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not close Streamwindow: " + e);
                    Console.WriteLine("");
                }

            }
        }

        public static Streamer[] OnlineFinder(Streamer[] stats, String website)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                website = website.Substring(website.IndexOf("online-status") + 14);
                if (stats[i].Completed == false && website.Substring(0, 7) == "is-live")
                {
                    stats[i].online = true;
                }
                else
                {
                    stats[i].online = false;
                }
            }
            return stats;
        }

        public static Streamer[] Get_status(string website, int j)
        {

            Streamer[] stats = new Streamer[j];
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i] = new Streamer { Completed = false, Watchtime = 0 };
                website = website.Substring(website.IndexOf("<a href=\"https://") + 9);
                stats[i].URL = website.Substring(0, website.IndexOf("\""));
                if (stats[i].URL.Substring(0, 19) != "https://www.youtube")
                {
                    website = website.Substring(website.IndexOf("streamer-name") + 15);
                    stats[i].Name = website.Substring(0, website.IndexOf("<"));
                }
                else
                {
                    stats[i].Name = stats[i].URL.Substring(26, (stats[i].URL.Length - 27));
                }
            }
            return stats;
        }

        public static int CountStringOccurrences(string text, string pattern)
        {
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }

        public static Streamer[] update_stats()
        {
            string json = File.ReadAllText("stats.json");
            return JsonConvert.DeserializeObject<Streamer[]>(json);
        }

        public static void Save_progress(Streamer[] stats)
        {
            string json = JsonConvert.SerializeObject(stats, Formatting.Indented);
            File.WriteAllText("stats.json", json);
        }

        public static string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    public class Streamer
    {
        public int Watchtime { get; set; }
        public String Name { get; set; }
        public String URL { get; set; }
        public bool Completed { get; set; }
        public int Priority { get; set; }
        public bool online { get; set; }
    }

    class StreamerComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return (new CaseInsensitiveComparer()).Compare(((Streamer)x).Priority, ((Streamer)y).Priority);
        }
    }
}
