﻿using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Reflection;

namespace Ra3_Mod_Manager
{


    static class Program
    {
      

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {

            Debug.WriteLine("String Args Number:" + args.Length);
            

            for (int i = 0; i < args.Length; i++)
            {

                Debug.WriteLine("String Args Content:" + args[i].ToString());
                Debug.WriteLine("Running by:");

                if (args[i].Trim().ToLower().IndexOf("-help") != -1 || args[i].Trim().ToLower().IndexOf("/?") != -1)
                {
                    Debug.WriteLine(" -help or /?");
                    String data = "-help or /?:Message This Help.\n-ui:Open This Control Panel.\n-debug:Enable Debug Mode When Game Start.\n-script:Inject Data From \\Scripts.\n-skip:Skip Splash.\n-hook:Hook Dll From \\Hooks.\n-dat:Load Dat From \\Custom.dat.";
                    //Console.WriteLine(data);
                    MessageBox.Show(data, "Can Use:", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    System.Environment.Exit(0);
                    continue;
                }

                if (args[i].Trim().ToLower().IndexOf("-ui") != -1)
                {
                    Debug.WriteLine(" -ui");
                    Config.isDevloper = true;
                    continue;
                }

                if (args[i].Trim().ToLower().IndexOf("-debug") != -1)
                {
                    Debug.WriteLine(" -debug");
                    Config.isDebug = true;
                    Config.isDevloper = true;
                    Config.extraTitle += "DEBUG:";
                    continue;
                }



                if (args[i].Trim().ToLower().IndexOf("-script") != -1)
                {
                    Debug.WriteLine(" -script");
                    Config.canInj = true;
                    Config.extraTitle += "SCRIPT:";
                    continue;
                }

                if (args[i].Trim().ToLower().IndexOf("-skip") != -1)
                {
                    Debug.WriteLine(" -skip");
                    Config.canSkip = true;
                    Config.extraTitle += "SKIP:";
                    continue;
                }

                if (args[i].Trim().ToLower().IndexOf("-dat") != -1)
                {
                    String temp = args[i].Trim().ToLower();
                    Debug.WriteLine(" -dat");
                    Config.customDat = temp.Substring(temp.IndexOf("-dat") + 4, 10);
                    //Config.extraTitle += "DAT:";
                    continue;
                }


               

                if (args[i].Trim().ToLower().IndexOf("-hook") != -1)
                {
                     Debug.WriteLine(" -hook");
                    Config.canHook = true;
                    Config.extraTitle += "HOOK:";
                    continue;
                }
                
                Debug.WriteLine("");


            }


            Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName);
            Debug.WriteLine("Check Same:" + Process.GetCurrentProcess().ProcessName);
            if (!mutex.WaitOne(0,false))
            {
                Mutex.OpenExisting(Process.GetCurrentProcess().ProcessName);

                if (Config.customDat ==null) {
                    Config.readDAT(Application.StartupPath + "\\" + Config.configFile);
                }
                else
                {
                    Config.readDAT(Application.StartupPath + "\\Custom.dat\\" + Config.customDat+".dat");
                }


                MessageBox.Show(loc.in_running[loc.current], loc.con_title[loc.current], MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Environment.Exit(0);
            }


            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;


            Initialization();






            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Config.isFirstTime || Config.isDevloper || !File.Exists(Application.StartupPath + "\\"+Config.configFile)) {
                Config.mainController = new Controller();
                Application.Run(Config.mainController);
            }
            else
            {
                startGameByProcess();
                if (!Config.canSkip) { 
                Application.Run(new Splash_Form());
                }
            }
        }


        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string resourceName = "Ra3_Mod_Manager." + new AssemblyName(args.Name).Name + ".dll";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                byte[] assemblyData = new byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }

        static void checkFileExists()
        {



            String[] fileArray = { "\\Ra3.exe", "\\Data\\WBData.big", "\\Data\\Apt.big" };

            for (int i = 0; i < fileArray.Length; i++)
            {



                if (!File.Exists(Application.StartupPath + fileArray[i]))
                {

                    if (i == 0)
                    {
                        if (File.Exists(Application.StartupPath + "\\RA3EP1.exe"))
                        {

                            Config.isDLC = true;
                            Config.gameName = "Uprising";

                            for(int b = 0; b < loc.inf.Length; b++)
                            {
                                loc.in_game[b] = loc.in_game[b] + " " + loc.in_game_dlc[b];
                            }

                            continue;
                        }
                    }


                    try
                    {

                        CultureInfo ci = Thread.CurrentThread.CurrentCulture;

                        String inf = loc.in_notfound[loc.current] + Application.StartupPath + fileArray[i];
                        String title = loc.in_error[loc.current];

                        MessageBox.Show(inf, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Debug.WriteLine("Not Game Path:" + Application.StartupPath + fileArray[i]);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), loc.in_execption[loc.current], MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);


                    }
                    finally {
                        System.Environment.Exit(0);
                    }
                }
            }





        }



        static void Initialization()
        {
            firstTimeRun();
            checkFileExists();
            checkImageExists();
            Config.searchLanguage();
            //checkCustomerImageExists();
            if (Config.customDat == null)
            {
                Config.readDAT(Application.StartupPath + "\\" + Config.configFile);
            }
            else
            {
                Config.readDAT(Application.StartupPath + "\\Custom.dat\\" + Config.customDat + ".dat");
            }
            if (!checkMainModExists())
            {
                Config.runMode = 0;
                Config.gameList.Add(Config.gameName);
                Config.skudefList.Insert(0, Config.searchSkudef(Application.StartupPath));
                Config.modPathList.Insert(0, null);
                //Config.modPath = null;

                if (!Config.isDLC) {
                    Directory.CreateDirectory(Application.StartupPath + "\\Mods");
                    searchModFromDirectory(Application.StartupPath + "\\Mods");

                    Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\Red Alert 3\\Mods");
                    searchModFromDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\Red Alert 3\\Mods");
                }
            }


        }

        static void firstTimeRun()
        {

            if (!File.Exists(Application.StartupPath + "\\"+Config.configFile))
            {
                Config.isFirstTime = true;
                Debug.WriteLine("First Time Run!");
            }
            else
            {
                return;
            }


            

            String str = Config.systemLanguage.Parent.EnglishName.ToLower();
            Debug.WriteLine("Guess Language:" + str);
            int l = 0;
            if (str.IndexOf("chinese")!=-1)
            {

                if (str.IndexOf("simpl") != -1)
                {
                    l = 1;
                }
                else
                {
                    l = 2;
                }

            }

            String t = loc.inf[l] + "?";


            if (l != 0) { 
            DialogResult dr = MessageBox.Show(t,loc.set_lang_title[l],MessageBoxButtons.YesNo,MessageBoxIcon.Question);
           
            if (dr == DialogResult.Yes)
            {
                loc.current = l;

            }
            else
            {


              

                loc.current = 0 ;
            }
            }
            else
            {
                loc.current = 0;
            }
            //MessageBox.Show(loc.inf[loc.current], loc.inf[loc.current], MessageBoxButtons.YesNo, MessageBoxIcon.Information);


        }


        static void searchModFromDirectory(String path)
        {
            Debug.WriteLine("Search Mod Directory:" + path);

            String[] dirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < dirs.Length; i++)
            {

                Config.gameList.Add(Path.GetFileNameWithoutExtension(dirs[i]));
                Config.skudefList.Add(Config.searchSkudef(dirs[i]));
                Config.modPathList.Add(dirs[i]);

            }

        }




        /*
        static void checkCustomerImageExists()
        {
            String[] fileArray = { "\\Launcher\\MyTitle", "\\Launcher\\MySplash" };
            String[] fileNameExtension = { ".png", ".jpg", ".jpeg", ".bmp" };

            for (int i = 0; i < fileArray.Length; i++)
            {
                for (int x = 0; x < fileNameExtension.Length; x++)
                {
                    String path = Application.StartupPath + fileArray[i] + fileNameExtension[x];

                    if (File.Exists(path))
                    {
                        Config.isCustomImageMode = true;
                        Config.customerImage[i] = new Bitmap(path);
                        Config.currentImage[i] = Config.customerImage[i];
                        break;
                    }
                }
            }

        */
        static void checkModImageExists()
        {
            String[] fileArray = { "\\Controller", "\\Splash" };
            String[] fileNameExtension = { ".png", ".jpg", ".jpeg", ".bmp" };

            for (int i = 0; i < fileArray.Length; i++)
            {
                for (int x = 0; x < fileNameExtension.Length; x++)
                {
                    String path = Config.modPath + fileArray[i] + fileNameExtension[x];

                    if (File.Exists(path))
                    {


                        Config.currentImage[i] = new Bitmap(path);
                        break;
                    }
                }
            }





        }
        

        static bool checkMainModExists()
        {
            String[] dirs = Directory.GetDirectories(Application.StartupPath, "*.Mod", SearchOption.TopDirectoryOnly);
            if (dirs.Length != 0)
            {
                String dir = dirs[0];
                //Config.isCustomImageMode = false;
                Config.runMode = 1;
                Config.modPath = dir;
                Config.modPathList.Insert(0,dir);
                Debug.WriteLine("Main Mod Path:" + dir.ToString());
                Config.gameList.Add(Path.GetFileNameWithoutExtension(dir));
                Config.skudefList.Insert(0, Config.searchSkudef(dir));
                String[] fileArray = { "\\Controller", "\\Splash" };
                String[] fileNameExtension = { ".png", ".jpg", ".jpeg", ".bmp" };

                for (int i = 0; i < fileArray.Length; i++)
                {
                    for (int x = 0; x < fileNameExtension.Length; x++)
                    {
                        String path = dir + fileArray[i] + fileNameExtension[x];

                        if (File.Exists(path))
                        {

                            Config.modImage[i] = new Bitmap(path);
                            Config.currentImage[i] = Config.modImage[i];
                            break;
                        }
                    }
                }






            }
            else
            {
                return false;
            }

            return true;

        }


        static void checkImageExists()
        {
            Bitmap t, s;
            String tp = Application.StartupPath + "\\Launcher\\cnc.bmp", sp = Application.StartupPath + "\\Launcher\\splash.bmp";



            if (File.Exists(tp))
            {
                t = new Bitmap(tp);
                Debug.WriteLine("Original Image Found:" + tp);
            }
            else
            {
                t = null;
            }

            if (File.Exists(tp))
            {
                s = new Bitmap(sp);
                Debug.WriteLine("Original Image Found:" + sp);
            }
            else
            {
                s = null;
            }


            Config.originalImage[0] = t;
            Config.originalImage[1] = s;





        }



        private static void startGameByProcess()
        {
            //Process p = new Process();

            String targetMod = Config.dat_game;
            String targetSkudef = Config.dat_version + ".skudef";
            String targetVersion = Config.dat_version;
            StringBuilder command = new StringBuilder("");

            Debug.WriteLine("Confirm Game:" + targetMod);
            Debug.WriteLine("Confirm Version:" + targetVersion);

            if (Config.dat_win || Config.dat_bfs)
            {
                command.Append(" -win");
            }

            if (Config.dat_cr || Config.dat_bfs)
            {
                
                String xres = Config.dat_xres;
                String yres = Config.dat_yres;

                if (Config.dat_bfs)
                {
                    xres = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width.ToString();
                    yres = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height.ToString();

                    Debug.WriteLine("Need Use Desktop Resolution");

                }

                if (xres.Length > 0 && yres.Length > 0)
                {
                    command.Append(" -xres " + xres + " -yres " + yres);
                }
            }

            String game = "ra3_1.0.game";
            String customGame = "";
            if (targetMod.Equals(Config.gameName))
            {
                targetSkudef = "ra3_" + Config.dat_language + "_" + targetVersion + ".skudef";

                game = "ra3_" + targetVersion + ".game";
                command.Append(" -config \"" + Application.StartupPath + "\\" + targetSkudef + "\"");
            }
            else
            {
                Config.modPath = Config.dat_modpath;
                checkModImageExists();
                Debug.WriteLine("Mod Game Found:" + Config.modPath + "\\" + targetSkudef);
                String modGame = Config.readFirstLine(Config.modPath + "\\" + targetSkudef);

                

                /*
                String customGamePath = Config.modPath + "\\game.txt";

                if (File.Exists(customGamePath))
                {
                    customGame = Config.readFirstLine(customGamePath);

                }
                */

                if (modGame.Length <= 0 || modGame.IndexOf("mod-game") == -1)
                {
                    modGame = "1.12";
                    Debug.WriteLine("Unknown Mod Game Start Version:Set to" + modGame);
                    targetSkudef = "ra3_" + Config.dat_language + "_" + modGame + ".skudef";
                    game = "ra3_" + modGame + ".game";
                    command.Append(" -config \"" + Application.StartupPath + "\\" + targetSkudef + "\"");

                }
                else
                {
                    modGame = modGame.Substring(modGame.LastIndexOf("mod-game") + 8).Trim();
                    Debug.WriteLine("Mod Game Start Version:" + modGame);
                    String targetModSkudef = "ra3_" + Config.dat_language + "_" + modGame + ".skudef";
                    game = "ra3_" + modGame + ".game";
                    command.Append(" -config \"" + Application.StartupPath + "\\" + targetModSkudef + "\"");
                    command.Append(" -modConfig \"" + Config.modPath + "\\" + targetSkudef + "\"");
                }



            }

            Debug.WriteLine("Start Game:" + Application.StartupPath + "\\data\\" + game + command);

            Process main = new Process();
            Config.gameProcess = main;
            main.StartInfo.UseShellExecute = false;
            main.StartInfo.WorkingDirectory = Application.StartupPath + "\\data\\";

            if (Config.isDebug)
            {
                main.StartInfo.RedirectStandardError = true;
                main.StartInfo.RedirectStandardInput = true;
                main.StartInfo.RedirectStandardOutput = true;
            }

            if (File.Exists(Config.modPath + "\\" + "ra3_" + customGame + ".game"))
            {

                main.StartInfo.FileName = Config.modPath + "\\" + "ra3_" + customGame + ".game";
                Debug.WriteLine("Custom Game File: " + main.StartInfo.FileName);
                
                if (!Directory.Exists(Config.modPath + "\\Data\\Cursors"))
                {
                    Directory.CreateDirectory(Config.modPath + "\\Data\\Cursors");

                    String[] fg = Directory.GetFiles(Application.StartupPath + "\\Data\\Data\\Cursors", "*.ani", SearchOption.TopDirectoryOnly);

                    foreach(var i in fg)
                    {

                        File.Copy(i, Config.modPath + "\\Data\\Cursors\\" + Path.GetFileName(i));

                    }

                }

            }
            else
            {

                main.StartInfo.FileName = "data\\" + game;
                Debug.WriteLine("Orignial Game File: " + main.StartInfo.FileName);
            }

            Config.md5 = Config.getMd5(main.StartInfo.FileName);

            main.StartInfo.Arguments = command.ToString();

            main.Start();
            if (Config.canHook)
            {
                inj.dohook();
            }
            if (Config.canInj)
            {

                inj.doinj();
            }
            Game.doGameAction();
        }



    }



    public static class Config

    {
        public static String customDat = null;
        public static bool canSkip = false;
        public static String configFile = "Ra3_Mod_Manager.dat";
        public static String extraTitle = "";
        public static bool isFirstTime = false;
        public static bool isDebug = false;
        public static String gameName = "Red Alert 3";
        public static bool isDLC = false;
        public static bool isDevloper = false;
        public static Image[] originalImage = { null, null };
        //public static Image[] customerImage = new Bitmap[originalImage.Length];
        public static Image[] currentImage = { originalImage[0], originalImage[1] };
        public static Image[] modImage = new Bitmap[originalImage.Length];
        public static List<String> languageList = new List<String>();
        public static List<String[]> skudefList = new List<String[]>();
        public static List<String> gameList = new List<String>();
        public static List<String> modPathList = new List<String>();
        public static int runMode = 0; // 0 = normal;1 = unique;
        public static String modPath;
        public static Icon defaultIcon;
        public static CultureInfo systemLanguage = Thread.CurrentThread.CurrentCulture;

        //public static bool isCustomImageMode = false;
        public static String exeVersion = Application.ProductVersion.ToString();


        public static bool dat_win = false;
        public static bool dat_cr = false;
        public static String dat_xres = "800";
        public static String dat_yres = "600";
        public static String dat_game = "";
        public static String dat_version = "";
        public static String dat_language = "";
        public static String dat_modpath = "";
        public static int dat_loc = 0;
        public static bool dat_bfs = false;
        public static bool dat_media = false;
        public static Color btn_mb;
        public static Color btn_db;
        public static Image btnImage;
        public static Image btnImageMove;
        public static Image btnImageClick;
        public static String btnAudioMove = "";
        public static String btnAudioClick = "";
        public static Process gameProcess;
        public static Controller mainController;
        public static String timeStamp = DateTime.Now.ToString("yyymmddhhmmss");
        public static bool canInj = false;
        public static bool canHook = false;
        public static String md5;

        public static String getMd5(String path)
        {
            String smd5="";



            try
            {
                FileStream file = new FileStream(path, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                smd5 = sb.ToString();
            }
            catch (Exception)
            {

            }

            Debug.WriteLine("Try Get File MD5:" + smd5);
            return smd5;
        }

        public static void readDAT(String filepath)
        {
            Debug.WriteLine("Try Read Dat:" + filepath);
            if (Config.isFirstTime)
            {
                /*
                Ra3_Mod_Manager.Properties.Settings.Default.dat_loc = loc.current;
                Ra3_Mod_Manager.Properties.Settings.Default.Save();
                */

                Config.dat_loc = loc.current;
                //Config.writeDAT(false,false,"800","600","","","","",false,dat_loc);


            return;

            }
            


            

            if (!File.Exists(filepath))
            {
                return;
            }

            

            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);

           
            StreamReader sr = new StreamReader(fs);

            String str = sr.ReadLine();
            String[] option = str.Split('|');

            Debug.WriteLine("Read Dat:" + filepath);
            Debug.WriteLine("Read Data:" + str);
            try
            {
            dat_win = Boolean.Parse(option[0]);
            dat_cr = Boolean.Parse(option[1]);
            dat_xres = option[2];
            dat_yres = option[3];
            dat_game = option[4];
            dat_version = option[5];
            dat_language = option[6];
            dat_modpath = option[7];
            dat_media = Boolean.Parse(option[8]);
            dat_loc = int.Parse(option[9]);
            dat_bfs = bool.Parse(option[10]);
                loc.current = dat_loc;
                sr.Close();
            
            }catch(Exception e){
                MessageBox.Show(e.ToString(), "Exception:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                sr.Close();
                fs.Close();
                File.Delete(Application.StartupPath + "\\Ra3_Manager.dat");
            }


            

            /*

            dat_win = Ra3_Mod_Manager.Properties.Settings.Default.dat_win;
            dat_cr = Ra3_Mod_Manager.Properties.Settings.Default.dat_cr;
            dat_xres = Ra3_Mod_Manager.Properties.Settings.Default.dat_xres;
            dat_yres = Ra3_Mod_Manager.Properties.Settings.Default.dat_yres;
            dat_game = Ra3_Mod_Manager.Properties.Settings.Default.dat_game;
            dat_version = Ra3_Mod_Manager.Properties.Settings.Default.dat_version;
            dat_language = Ra3_Mod_Manager.Properties.Settings.Default.dat_language;
            dat_modpath = Ra3_Mod_Manager.Properties.Settings.Default.dat_modpath;
            dat_media = Ra3_Mod_Manager.Properties.Settings.Default.dat_media;
            dat_loc = Ra3_Mod_Manager.Properties.Settings.Default.dat_loc;

            loc.current = dat_loc;

            StringBuilder strBuild = new StringBuilder("");

            strBuild.Append(dat_win);
            strBuild.Append('|');
            strBuild.Append(dat_cr);
            strBuild.Append('|');
            strBuild.Append(dat_xres);
            strBuild.Append('|');
            strBuild.Append(dat_yres);
            strBuild.Append('|');
            strBuild.Append(dat_game);
            strBuild.Append('|'); ;
            strBuild.Append(dat_version);
            strBuild.Append('|');
            strBuild.Append(dat_language);
            strBuild.Append('|');
            strBuild.Append(dat_modpath);
            strBuild.Append('|');
            strBuild.Append(dat_media);
            strBuild.Append('|');
            strBuild.Append(dat_loc);


            Debug.WriteLine("Read Data:" + strBuild.ToString());
            */

        }

        public static String writeDAT(String filepath,bool win, bool cr, String xres, String yres, String game, String version, String language, String modPath, bool media,int loc,bool bfs)
        {
            
             FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write);
             StreamWriter sw = new StreamWriter(fs);
             
            StringBuilder strBuild = new StringBuilder("");
             
             strBuild.Append(win);
             strBuild.Append('|');
             strBuild.Append(cr);
             strBuild.Append('|');
             strBuild.Append(xres);
             strBuild.Append('|');
             strBuild.Append(yres);
             strBuild.Append('|');
             strBuild.Append(game);
             strBuild.Append('|'); ;
             strBuild.Append(version);
             strBuild.Append('|');
             strBuild.Append(language);
             strBuild.Append('|');
             strBuild.Append(modPath);
             strBuild.Append('|');
             strBuild.Append(media);
             strBuild.Append('|');
             strBuild.Append(loc);
            strBuild.Append('|');
            strBuild.Append(bfs);

            Debug.WriteLine("Write Data:" +strBuild.ToString());
            
            Debug.WriteLine("Write Dat:"+filepath);
           sw.BaseStream.Seek(0, SeekOrigin.End);
           sw.WriteLine(strBuild.ToString());
           sw.Flush();

           sw.Close();

            return strBuild.ToString();

            //System Settings
           

            /*
            Ra3_Mod_Manager.Properties.Settings.Default.dat_win = win;
            Ra3_Mod_Manager.Properties.Settings.Default.dat_cr = cr;
            Ra3_Mod_Manager.Properties.Settings.Default.dat_xres = xres;
            Ra3_Mod_Manager.Properties.Settings.Default.dat_yres = yres;
            Ra3_Mod_Manager.Properties.Settings.Default.dat_game = game;
            Ra3_Mod_Manager.Properties.Settings.Default.dat_version = version;
            Ra3_Mod_Manager.Properties.Settings.Default.dat_language = language;
            Ra3_Mod_Manager.Properties.Settings.Default.dat_modpath = modPath;
            Ra3_Mod_Manager.Properties.Settings.Default.dat_media = media;
            Ra3_Mod_Manager.Properties.Settings.Default.dat_loc = loc;
            Ra3_Mod_Manager.Properties.Settings.Default.Save();

        */


        }

        public static String[] searchSkudef(String modPath)
        {
            String[] skudef = Directory.GetFiles(modPath, "*.skudef", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < skudef.Length; i++)
            {

                Debug.WriteLine("Found SkuDef:" + skudef[i]);

                skudef[i] = Path.GetFileNameWithoutExtension(skudef[i]);

            }

            return skudef;

        }

        public static void searchLanguage()
        {
            String[] skudef = Directory.GetFiles(Application.StartupPath, "*.skudef", SearchOption.TopDirectoryOnly);
            List<String> availiable = new List<String>();
            for (int i = 0; i < skudef.Length; i++)
            {
                bool isAlive = false;
                int start = skudef[i].IndexOf('_') + 1;
                int end = skudef[i].LastIndexOf('_');

                skudef[i] = skudef[i].Substring(start, end - start);


                for (int x = 0; x < availiable.Count; x++)
                {
                    if (availiable[x].Equals(skudef[i]))
                    {
                        isAlive = true;

                        break;
                    }

                }

                if (!isAlive)
                {
                    languageList.Add(skudef[i]);
                    availiable.Add(skudef[i]);
                }
            }


        }



        public static String readFirstLine(String path)
        {

            Debug.WriteLine("Read First Line From Path:" + path);
            FileStream fs = new FileStream(path, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            String str = sr.ReadLine();

            try
            {
                sr.Close();

            } catch (Exception e) {
                MessageBox.Show(e.ToString(), loc.in_execption[loc.current], MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            return str;

        }









    }



    public static class loc{
    public static String[] inf = {"English","简体中文","繁體中文"};
    public static int current = 2;

        public static String[]

            con_title = new String[inf.Length],
            btn_information = new String[inf.Length],
            btn_modfolder = new String[inf.Length],
            btn_ra3ui = new String[inf.Length],
            btn_saveoption = new String[inf.Length],
            btn_startgame = new String[inf.Length],
            btn_website = new String[inf.Length],
            cb_customresolution = new String[inf.Length],
            cb_media = new String[inf.Length],
            cb_windowed = new String[inf.Length],
            in_author = new String[inf.Length],
            in_error = new String[inf.Length],
            in_execption = new String[inf.Length],
            in_game = new String[inf.Length], in_game_dlc = new String[inf.Length],
            in_information = new String[inf.Length],
            in_noinf = new String[inf.Length],
            in_notfound = new String[inf.Length],
            lb_game = new String[inf.Length],
            lb_height = new String[inf.Length],
            lb_version = new String[inf.Length],
            lb_width = new String[inf.Length],
            in_running = new String[inf.Length],
            cb_bfs = new String[inf.Length],
            set_lang_title = new String[inf.Length],
            btn_map = new String[inf.Length],
            btn_document = new String[inf.Length],
            in_notDir = new string[inf.Length],
            btn_shortcut = new string[inf.Length]

            ;


        
        

        static loc() {
            int i = 0;
            //English
            con_title[i] = "Command & Conquer:Red Alert 3 Mod Manager";
            btn_information[i] = "Introduce";
            btn_modfolder[i] = "Mod Folder";
            btn_ra3ui[i] = "Ra3 Panel";
            btn_saveoption[i] = "Save Options";
            btn_startgame[i] = "Start Game";
            btn_website[i] = "Web Site";
            cb_customresolution[i] = "Custom Resolution";
            cb_media[i] = "Media";
            cb_windowed[i] = "Windowed";
            in_author[i] = "Red Alert 3 Mod Manager Author:HaoJun0823 from China Version:"+ Config.exeVersion;
            in_error[i] = "Error:";
            in_execption[i] = "Execption:";
            in_game[i] = "Command & Conquer:Red Alert 3";
            in_game_dlc[i] = "Uprising";
            in_information[i] = "Information:";
            in_noinf[i] = "This mod doesn't have any introduce";
            in_notfound[i] ="Please move this application into the Red Alert 3 Game Directory.Error:Cannot find game file:";
            lb_game[i] = "Game:";
            lb_version[i] = "Version:";
            lb_height[i] = "Height:";
            lb_width[i] = "Width:";
            in_running[i] = "I'm running, don't open the second.";
            cb_bfs[i] = "Borderless Windowed (Full Screen)";
            set_lang_title[i] = "Language Chooser";
            btn_map[i] = "Maps";
            btn_document[i] = "Replays";
            in_notDir[i] = "Directory is not exists:";
            btn_shortcut[i] = "Create Shortcut";

            i++;
            //CS
            con_title[i] = "命令与征服：红色警戒3模组管理器";
            btn_information[i] = "介绍";
            btn_modfolder[i] = "数据目录";
            btn_ra3ui[i] = "控制中心";
            btn_saveoption[i] = "保存配置";
            btn_startgame[i] = "开始游戏";
            btn_website[i] = "模组网站";
            cb_customresolution[i] = "自定义分辨率";
            cb_media[i] = "多媒体";
            cb_windowed[i] = "窗口化";
            in_author[i] = "红色警戒3模组管理器 作者：HaoJun0823 来自中国 版本：" + Config.exeVersion;
            in_error[i] = "错误：";
            in_execption[i] = "异常：";
            in_game[i] = "命令与征服：红色警戒3";
            in_game_dlc[i] = "起义时刻";
            in_information[i] = "信息：";
            in_noinf[i] = "这个模组没有任何介绍资料。";
            in_notfound[i] = "请将此运行程序移动至红色警戒3的游戏目录中。错误：没有找到游戏文件：";
            lb_game[i] = "游戏：";
            lb_version[i] = "版本：";
            lb_height[i] = "高度：";
            lb_width[i] = "宽度：";
            in_running[i] = "正在运行，不要开启第二个。";
            cb_bfs[i] = "无边框窗口化（全屏）";
            set_lang_title[i] = "语言选择器";
            btn_map[i] = "地图";
            btn_document[i] = "录像";
            in_notDir[i] = "目录不存在：";
            btn_shortcut[i] = "创建快捷方式";

            i++;
            //TS

            con_title[i] = "終極動員令：紅色警戒3模組管理器";
            btn_information[i] = "介紹";
            btn_modfolder[i] = "數據目錄";
            btn_ra3ui[i] = "控制中心";
            btn_saveoption[i] = "保存配置";
            btn_startgame[i] = "開始遊戲";
            btn_website[i] = "模組網站";
            cb_customresolution[i] = "自定義分辨率";
            cb_media[i] = "多媒體";
            cb_windowed[i] = "窗口化";
            in_author[i] = "紅色警戒3模組管理器 作者：HaoJun0823 來自中國 版本：" + Config.exeVersion;
            in_error[i] = "錯誤：";
            in_execption[i] = "異常：";
            in_game[i] = "終極動員令：紅色警戒3";
            in_game_dlc[i] = "起義時刻";
            in_information[i] = "信息：";
            in_noinf[i] = "這個模組沒有任何介紹資料。";
            in_notfound[i] = "請將此運行程序移動至紅色警戒3的遊戲目錄中。錯誤：沒有找到遊戲文件：";
            lb_game[i] = "遊戲：";
            lb_version[i] = "版本：";
            lb_height[i] = "高度：";
            lb_width[i] = "寬度：";
            in_running[i] = "正在運行，不要開啟第二個。";
            cb_bfs[i] = "無邊框窗口化（全屏）";
            set_lang_title[i] = "語言選擇器";
            btn_map[i] = "地圖";
            btn_document[i] = "錄像";
            in_notDir[i] = "目錄不存在：";
            btn_shortcut[i] = "創建快捷方式";
        }
    }




}
