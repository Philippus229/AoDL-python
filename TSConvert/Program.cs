/*
 AOD-Downloader Copyright (C) 2019  BreakingBread0
 This program comes with ABSOLUTELY NO WARRANTY.
 This is free software, and you are welcome to redistribute it
 under certain conditions.
 
 Please read the license for further information:
 https://www.gnu.org/licenses/gpl-3.0.en.html (ENGLISH)
 https://www.gnu.org/licenses/gpl-3.0.de.html (GERMAN)
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace TSConvert {
    static class Program {
        //OpenFileDialog file = new OpenFileDialog();
        //file.Filter = "TS Files | *.ts";
        //if (file.ShowDialog() == DialogResult.OK) {
        //    string outFile = Path.Combine(Path.GetDirectoryName(file.FileName), Path.GetFileNameWithoutExtension(file.FileName) + "_conv.mp4");
        //    Process.Start("ffmpeg", "-i \"" + file.FileName + "\" -acodec copy -vcodec copy -c copy \"" + outFile + "\"");
        //}
        static string FFMPEG_LOCATION = "ffmpeg-4.1.1-win64-static";


        public static string CleanIllegalName(string p_testName) => new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars())))).Replace(p_testName, "");

        static void ConvertFile(string f, string outFile) {
            Console.WriteLine(f);
            Console.WriteLine(outFile);
            //string outFile = Path.Combine(Path.GetDirectoryName(f), Path.GetFileNameWithoutExtension(f) + "_conv.mp4");
            Process p = Process.Start(new ProcessStartInfo(FFMPEG_LOCATION, "-y -i " + f + " -acodec copy -vcodec copy -c copy \"all.mp4\"")/* { RedirectStandardOutput = false, UseShellExecute = true }*/);
            p.WaitForExit();

            if (p.ExitCode == 0) {
                if (File.Exists(CleanIllegalName(outFile)))
                    File.Delete(CleanIllegalName(outFile));
                File.Move("all.mp4", CleanIllegalName(outFile));
            } else {
                Console.WriteLine("Convert failed");
                throw new Exception("Convert failed! Exit code: " + p.ExitCode);
            }
            //Console.WriteLine(p.StandardOutput.ReadToEnd());
            //Console.ReadKey();
        }

        static WebCookieClient w = new WebCookieClient();
        //https://www.anime-on-demand.de/users/sign_in
        static string GET(string site) {
            try {
                return w.DownloadString(site);
            } catch (WebException ex) {
                String responseFromServer = ex.Message.ToString() + " ";
                Console.WriteLine(responseFromServer);
                if (ex.Response != null) {
                    responseFromServer += new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    return responseFromServer;
                } else {
                    return "";
                }
            }
        }

        static string POST(string site, NameValueCollection postData) {
            try {
                return Encoding.UTF8.GetString(w.UploadValues(site, postData));
            } catch (WebException ex) {
                String responseFromServer = ex.Message.ToString() + " ";
                Console.WriteLine(responseFromServer);
                if (ex.Response != null) {
                    responseFromServer += new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    return responseFromServer;
                } else {
                    return "";
                }
            }
        }

        //name="authenticity_token" value="
        static string[] Explode(string str, string explparam) {
            try {
                return System.Text.RegularExpressions.Regex.Split(str, explparam);
            } catch {
                return new string[0];
            }
        }
        static string Explode(string str, string explparam, int index) {
            try {
                return System.Text.RegularExpressions.Regex.Split(str, explparam)[index];
                //return str.Substring(str.IndexOf(explparam));
            } catch {
                //Console.WriteLine("Explode failed!");
                return "";
            }
        }

        public static string user, pass;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Console.Title = "AOD Downloader";

            w.Encoding = Encoding.UTF8;
            if (!Directory.Exists(FFMPEG_LOCATION)) {
                Console.WriteLine("Downloade Datein...");

                w.DownloadFile("https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-4.1.1-win64-static.zip", "release.zip");
                ZipFile.ExtractToDirectory("release.zip", Directory.GetCurrentDirectory());
                File.Delete("release.zip");
            }
            FFMPEG_LOCATION = Path.Combine(FFMPEG_LOCATION, "bin\\ffmpeg.exe");
            //Console.Write("Benutzer > ");
            //string user = Console.ReadLine();
            //Console.Write("Passwort > ");
            //string pass = Console.ReadLine();


            Console.WriteLine("Warten auf Auth Token...");
            GET("https://anime-on-demand.de");
            string auth_token = GET("https://www.anime-on-demand.de/users/sign_in");
            auth_token = Explode(auth_token, "name=\"authenticity_token\" value=\"", 1);
            auth_token = Explode(auth_token, "\"", 0);

            Console.WriteLine("Auth Token: " + auth_token);

            new LoginForm().ShowDialog();
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) {
                Environment.Exit(0);
            }

            NameValueCollection login = new NameValueCollection() {
                { "utf8", "✓" },
                { "authenticity_token", auth_token },
                { "user[login]", user },
                { "user[password]", pass },
                { "user[remember_me]", "1" },
                { "commit", "Einloggen" }
            };
            string result = POST("https://www.anime-on-demand.de/users/sign_in", login);
            if (result.Contains("Hallo, du bist jetzt angemeldet.")) {
                Console.WriteLine("Eingeloggt!");
            } else {
                Console.WriteLine(result);
                Console.ReadKey();
                return;
            }

            Console.Clear();

            string applied = GET("https://www.anime-on-demand.de/html5beta");
            if (applied.ToUpper().Contains("TEST ANMELDEN")) {
                GET("https://www.anime-on-demand.de/html5apply");
                Console.WriteLine("Der HTML5 Modus für Videos wurde aktiviert.");
            }

            string animes = GET("https://www.anime-on-demand.de/myanimes");

            Dictionary<string, string> animeList = new Dictionary<string, string>();
            int index = 0;
            string anime;
            while ((anime = Explode(animes, "animebox-title\">", 1 + index)) != string.Empty) {
                //<a href="/anime/
                //https://www.anime-on-demand.de/anime/***

                string title = Explode(anime, "<", 0);
                string link = Explode(anime, "<a href=\"/anime/", 1);
                link = Explode(link, "\"", 0);
                animeList.Add(title, link);
                index++;
            }

            getanime:
            //==========
            Console.WriteLine("==========Anime Liste==========");
            for (int i = 0; i < animeList.Count; i++) {
                var k = animeList.ElementAt(i);
                Console.WriteLine(i + ": " + k.Key + " (" + k.Value + ")");
            }

            Console.Write("Anime > ");
            //int currAnime = ;
            var curr = animeList.ElementAt(int.Parse(Console.ReadLine()));

            Console.WriteLine("==========" + curr.Key + "==========");

            Dictionary<string, string> Episoden = new Dictionary<string, string>();
            //class="episodebox-title" title="
            index = 0;
            string sEpisoden = GET("https://www.anime-on-demand.de/anime/" + curr.Value);

            //<meta name="csrf-token" content="
            string CSRF = Explode(sEpisoden, "<meta name=\"csrf-token\" content=\"", 1);
            CSRF = Explode(CSRF, "\"", 0);


            int index2 = 0;


            string epi;
            while ((epi = Explode(sEpisoden, "class=\"episodebox-title\" title=\"", 1 + index)) != string.Empty) {
                string title = Explode(epi, "\"", 0);

                //streamstarter_html5" title="
                //data-playlist="
                //Japanischen Stream mit Untertiteln
                var PlayType = Explode(epi, "streamstarter_html5\" title=\"");
                if (PlayType.Length > 1) {
                    for (int i = 1; i < PlayType.Length; i++) {

                        string type;
                        if (PlayType[i].Contains("Japanischen Stream mit Untertiteln")) {
                            type = "UT";
                        } else {
                            type = "GER";
                        }
                        //Console.WriteLine(index2 + ": " + title + " - " + type);

                        string streamLink = Explode(PlayType[i], "data-playlist=\"", 1);
                        streamLink = Explode(streamLink, "\"", 0);
                        
                        Episoden.Add(title + " - " + type + ".mp4", streamLink);
                        index2++;
                    }
                }


                index++;
            }
            var ck = new ChEpi(curr.Key, Episoden);
            ck.ShowDialog();

            foreach (var item in ck.EpisodesToDownload) {
                DownloadEpisode(item.Value, CSRF, "https://www.anime-on-demand.de/anime/" + curr.Value, item.Key);
            }

            //Console.WriteLine("ALL: Downloade Alle Episoden");
            //Console.WriteLine("ZUR: Zurück zur Auswahl");

            //Console.Write("Episode > ");
            //string input = Console.ReadLine();
            //if (input.ToUpper().Equals("ALL")) {
            //    for (int i = 0; i < Episoden.Count; i++) {
            //        DownloadEpisode(Episoden.ElementAt(i).Value, CSRF, "https://www.anime-on-demand.de/anime/" + curr.Value, Episoden.ElementAt(i).Key);
            //    }
            //} else if (input.ToUpper().Equals("ZUR")) {
            //    goto getanime;
            //} else {
            //    DownloadEpisode(Episoden.ElementAt(int.Parse(input)).Value, CSRF, "https://www.anime-on-demand.de/anime/" + curr.Value, Episoden.ElementAt(int.Parse(input)).Key);
            //}
            goto getanime;
        }

        static string SubStrLast(this string input, char indicator) {
            int index = input.LastIndexOf(indicator);
            return index == -1 ? input : input.Substring(0, index);
        }

        public static void DownloadFile(string url, string filename) {
            using (var sr = new StreamReader(HttpWebRequest.Create(url).GetResponse().GetResponseStream()))
            using (var sw = new StreamWriter(filename)) {
                sw.Write(sr.ReadToEnd());
            }
        }

        static void DownloadEpisode(string link, string csrfToken, string referer, string output) {
            Console.WriteLine("CSRF: " + csrfToken);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://anime-on-demand.de" + link);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.86 Safari/537.36";
            req.Accept = "application/json, text/javascript, */*; q=0.01";
            req.Headers.Add("X-CSRF-Token", csrfToken);
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            req.Method = "GET";
            req.Host = "www.anime-on-demand.de";
            req.Referer = referer;
            req.ContentType = "application/json";
            req.CookieContainer = w._container;
            

            try {
                WebResponse response = req.GetResponse();
                StreamReader r = new StreamReader(response.GetResponseStream());
                //Console.WriteLine(r.ReadToEnd());

                JObject obj = JObject.Parse(r.ReadToEnd());
                string playlist = obj["playlist"][0]["sources"][0]["file"].ToObject<string>();
                //Console.WriteLine("Playlist: " + playlist);

                //Playlist:
                //#EXT-X-STREAM-INF:BANDWIDTH=13568000,RESOLUTION=1920x1081
                //chunklist_w567166729_b13568000... --> chunklist link
                string baseURL = playlist.SubStrLast('/') + "/";
                //TODO: List and let the user choose what stream quality.
                string chunkListURL = string.Empty;
                var playlistlines = Explode(GET(playlist), "\n");
                for (int i = 0; i < playlistlines.Length; i++) {
                    if (playlistlines[i].Contains("chunklist_")) {
                        chunkListURL = baseURL + playlistlines[i];
                        //Console.WriteLine("Highest Chunk URL: " + baseURL);
                        break;
                    }
                }
                var fullChunkList = Explode(GET(chunkListURL), "\n");
                var chunkList = (from a in fullChunkList where a.Contains("media_") select a).ToArray<string>();

                Chunks = chunkList.Length;
                currentChunks = 0;

                if (!Directory.Exists("temp"))
                    Directory.CreateDirectory("temp");

                var t = new Thread(() => {
                    try {
                        new ProgressForm().ShowDialog();
                    } catch { }
                });
                t.Start();


                ParallelOptions options = new ParallelOptions();
                options.MaxDegreeOfParallelism = 20;
                ServicePointManager.DefaultConnectionLimit = 10000;
                ServicePointManager.MaxServicePoints = 10000;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Parallel.For(0, chunkList.Length, options, (a) => {
                    new WebClient().DownloadFile(baseURL + chunkList[a], "temp\\" + a + ".ts");
                    Console.WriteLine("Chunk " + a + " Fertig!");
                    currentChunks++;
                    Console.Title = "AOD Downloader [" + currentChunks + "/" + Chunks + "]";
                });
                t.Abort();
                MuxFiles(Chunks, output, fullChunkList);
            } catch (WebException ex) {
                String responseFromServer = ex.Message.ToString() + " ";
                //Console.WriteLine(responseFromServer);
                if (ex.Response != null) {
                    responseFromServer += new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    Console.WriteLine(responseFromServer);
                }
                Console.ReadLine();
            } catch (Exception e) {
                Console.WriteLine("Could not complete request! Exception: " + Environment.NewLine + e.ToString());
                Console.ReadLine();
            }
            Console.ForegroundColor = ConsoleColor.White;
            File.Delete("all.ts");
            foreach (string file in Directory.GetFiles("temp")) {
                File.Delete(file);
            }
            Directory.Delete("temp");
            Console.Title = "AOD Downloader";
            Console.Clear();
        }

        public static void MuxFiles(int count, string file, string[] ChunkFile) {
            for (int i = 0; i < ChunkFile.Length; i++) {
                if (ChunkFile[i].Contains("_")) {
                    var c = Explode(ChunkFile[i], "_");
                    ChunkFile[i] = c[c.Length - 1];
                }
            }

            File.WriteAllLines("temp\\list.m3u8", ChunkFile);

            Process.Start(FFMPEG_LOCATION, "-y -i \"temp/list.m3u8\" -c copy all.ts").WaitForExit();
            ConvertFile("all.ts", file);
        }

        public static int Chunks, currentChunks;
    }
}
