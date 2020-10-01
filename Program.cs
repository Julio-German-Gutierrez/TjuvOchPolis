using System;
using System.Collections.Generic;
using System.Timers;

namespace TjuvOchPolis
{
    class Program
    {
        static int[,] stad; // Bi- dimensional array representing the stad. It takes its values from StadWidth and StadHeight.
        static int StadWidth { get { return 100; } } // 80
        static int StadHeight { get { return 30; } } // 24
        
        // Lists of characters.
        static List<Polis> poliser = new List<Polis>();
        static List<Tjuv> tjuvar = new List<Tjuv>();
        static List<Manniska> manniskor = new List<Manniska>();
        static int antalPoliser { get { return 10; } } // 10
        static int antalTjuvar { get { return 20; } } // 20
        static int antalMerdborgare { get { return 30; } } // 30
        
        // "polisraport" is a collection of messeges about the encounters. Like "Tjuv rånar medborgare" and so...
        // "handelser" is a list of coordinates and extra information that can be used to locate the encounters.
        static List<string> polisRaport = new List<string>();
        static List<int[]> handelser = new List<int[]>();
        
        // Main Timer class to call the Refresh of the simulation
        static Timer timer;

        static void Main(string[] args)
        {
            // We set Buffer size and Window's console size.
            Console.SetBufferSize(StadWidth + 1, StadHeight + 10);
            Console.SetWindowSize(StadWidth + 1, StadHeight + 15);
            Console.BackgroundColor = ConsoleColor.Black;

            Console.Clear();
            PrintIntro(); // Intro screen.

            // ClearStad():Fills the stad[,] with zeros. Zeros are interpreted as empty spaces by the PrintStad() method.
            ClearStad();

            //CreateCharacters() create the characters and fill each list with them.
            CreateCharacters(antalPoliser, antalTjuvar, antalMerdborgare);
            
            TimerStart(250);

            //Simple escape mechanism. It waits until a key is pressed.
            Console.ReadKey(true);
            timer.Stop();
            timer.Dispose();
        } // END OF MAIN METHOD


        private static void TimerStart(int interval)
        {
            timer = new Timer();
            timer.Interval = interval;
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
        }


        // Timer_Elapsed() is called after every interval in TimeStart().
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MoveStad(); // Calculate the position of every character and updates the stad[,] array.
            bool c = CheckCollitions(); // Check for collitions. Returns TRUE or FALSE accordingly.

            // PrintStad() prints the screen. using information from stad[,], polisRaport[] and haldelser[] arrays.
            // If collitions is TRUE, instead of printing the stad, it will print the location of the incidents
            // and the REPORTS in form of text strings.
            if (c) PrintStad(c);
            else PrintStad();

            // We always clear the reports and the händelser.
            polisRaport.Clear();
            handelser.Clear();
        }


        // ClearStad() clear stad[,] to zeros.
        private static void ClearStad()
        {
            stad = new int[StadHeight, StadWidth];
            for (int i = 0; i < stad.GetLength(0); i++)
            {
                for (int j = 0; j < stad.GetLength(1); j++)
                {
                    stad[i, j] = 0;
                }
            }
        }


        private static bool CheckCollitions()
        {
            timer.Stop(); // Stop the simulation while calculating.

            bool collisions = false; // Return value. Set to FALSE before the checking.
            foreach (Tjuv t in tjuvar)
            {
                Manniska toKill = null;
                foreach (Manniska m in manniskor)
                {
                    if (t.PositionX == m.PositionX && t.PositionY == m.PositionY )
                    {
                        collisions = true;
                        if (m.Inventory.Count != 0 || m.Pengar != 0)
                        {
                            string stolen = t.StealFrom(m);
                            int pen = m.Pengar;
                            t.Pengar += pen;
                            m.Pengar = 0;
                            handelser.Add(new int[] { t.PositionX, t.PositionY, 4 });
                            polisRaport.Add($"Tjuven rånade {stolen} från medborgare med PN: {m.PersonNummer}");
                            polisRaport.Add($"Tjuven rånade också {pen}kr. från medborgare.");
                            string inv = "Merborgares inventory: ";
                            foreach (Objekt opo in m.Inventory)
                            {
                                inv = inv + opo.ToString() + ", ";
                            }
                            polisRaport.Add(inv.Substring(0, inv.Length - 2) + "." );
                        }
                        else // Kill the poor guy.
                        {
                            toKill = m;
                            handelser.Add(new int[] { t.PositionX, t.PositionY, 4 });
                            polisRaport.Add($"Tjuven DÖDAR medborgare - PN: {m.PersonNummer}");
                        }
                    }
                }
                if(toKill != null) manniskor.Remove(toKill);
            }

            Polis polis = null;
            foreach (Polis p in poliser)
            {
                Tjuv remove = null;
                foreach (Tjuv t in tjuvar)
                {
                    if (p.PositionX == t.PositionX && p.PositionY == t.PositionY)
                    {
                        Random ran = new Random();
                        collisions = true;
                        if (ran.Next(0,100) < 5)
                        {
                            polis = p;
                            handelser.Add(new int[] { p.PositionX, p.PositionY, 5 });
                            polisRaport.Add($"Polisen DÖDAS av tjuven");
                        }
                        else
                        {
                            p.RecoverEverythingFrom(t);
                            remove = t;
                            handelser.Add(new int[] { p.PositionX, p.PositionY, 5 });
                            //encounters.Add($"Tjuven rånade {t.Inventory[t.Inventory.Count - 1]} från PID: {m.PersonNummer}");
                            //encounters.Add($"Polisen återfår stulna saker från en tjuv");
                            polisRaport.Add($"Polisen tar tjuven");
                        }
                    }
                }
                if(remove != null) tjuvar.Remove(remove);
            }
            if(polis != null) poliser.Remove(polis);
            timer.Start(); // Restart the simulation.
            return collisions;
        } // end of CheckCollitions()


        // MoveStad() calculates the displacements of the characters and updates the stad[,] array.
        private static void MoveStad()
        {
            timer.Stop();

            ClearStad();

            foreach (Manniska m in manniskor)
            {
                MoveCharacter(m);
            }
            foreach (Tjuv t in tjuvar)
            {
                MoveCharacter(t);
            }
            foreach (Polis p in poliser)
            {
                MoveCharacter(p);
            }
            timer.Start();
        }// End of MoveStad()


        // MoveCharacter() calculate the next movement of each character.
        private static void MoveCharacter(Medborgare m)
        {
            m.PositionX += m.DirectionX;
            if (m.PositionX > StadWidth - 1) m.PositionX = 0;
            if (m.PositionX < 0) m.PositionX = StadWidth - 1;

            m.PositionY += m.DirectionY;
            if (m.PositionY > StadHeight - 1) m.PositionY = 0;
            if (m.PositionY < 0) m.PositionY = StadHeight - 1;

            if (m is Manniska)
            {
                stad[m.PositionY, m.PositionX] = (int)TypeCitizen.Person;
            }
            else if (m is Tjuv)
            {
                stad[m.PositionY, m.PositionX] = (int)TypeCitizen.Thief;
            }
            else
            {
                stad[m.PositionY, m.PositionX] = (int)TypeCitizen.Polis;
            }
        }


        // Refresh the screen. Our main Print to screen method.
        private static void PrintStad(bool collitions = false)
        {
            timer.Stop();
            Console.Clear();

            if (collitions)
            {
                for (int i = 0; i < stad.GetLength(0); i++)
                {
                    for (int j = 0; j < stad.GetLength(1); j++)
                    {
                        stad[i, j] = 0;
                    }
                }

                foreach (int[] i in handelser)
                {
                    if(i[2] == 4) stad[i[1], i[0]] = 4; // Tjuv
                    else stad[i[1], i[0]] = 5; // Polis
                }
            }


            for (int i = 0; i < stad.GetLength(0); i++)
            {
                for (int j = 0; j < stad.GetLength(1); j++)
                {
                    if (stad[i, j] == 0)
                    {
                        Console.Write(" ");
                    }
                    else if (stad[i, j] == 1)
                    {
                        ConsoleColor oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write("P");
                        Console.ForegroundColor = oldColor;
                    }
                    else if (stad[i, j] == 2)
                    {
                        ConsoleColor oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("T");
                        Console.ForegroundColor = oldColor;
                    }
                    else if (stad[i, j] == 3)
                    {
                        ConsoleColor oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("M");
                        Console.ForegroundColor = oldColor;
                    }
                    else if (stad[i, j] == 4)
                    {
                        ConsoleColor oldColor = Console.ForegroundColor;
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("T");
                        Console.ForegroundColor = oldColor;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        ConsoleColor oldColor = Console.ForegroundColor;
                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("P");
                        Console.ForegroundColor = oldColor;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                }
                Console.WriteLine(); // WriteLine after each line in stad.
            }

            //Console.WriteLine($"Antal tjuvar: {tjuvar.Count}/{numberThieves}                                    Antal människor: {manniskor.Count}/{numberCitizens}");
            Console.WriteLine("{0,15}{1:00}/{2:00}{3,30}{4:00}/{5:00}{6,39}{7:00}/{8:00}","Antal tjuvar: ",tjuvar.Count,antalTjuvar,"Antal människor: ",manniskor.Count,antalMerdborgare,"Antal poliser: ",poliser.Count,antalPoliser);

            foreach (string s in polisRaport)
            {
                if (s.Contains("Polis"))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(s);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(s);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            timer.Start();

            // If collitions were detected, the simulation is stopped for the selected interval.
            // Giving time to read the collitions.
            if (collitions)
            {
                timer.Stop();
                Timer t = new Timer(3000);
                t.Elapsed += T_Elapsed;
                t.AutoReset = false;
                t.Start();
            }
        }// End of PrintStad()


        // After the collitions time is reached, we restart the simulation.
        private static void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Start();
        }


        private static void CreateCharacters(int np, int nt, int nc)
        {
            for (int i = 0; i < np; i++)
            {
                Polis p = new Polis();
                Random r = new Random();
                int x = r.Next(0, StadWidth);
                int y = r.Next(0, StadHeight);
                while (stad[y, x] != 0)
                {
                    x = r.Next(0, StadWidth);
                    y = r.Next(0, StadHeight);
                }
                p.PositionX = x;
                p.PositionY = y;
                p.DirectionX = RandomDirection();
                p.DirectionY = RandomDirection();
                poliser.Add(p);
                stad[y, x] = (int)TypeCitizen.Polis;
            }
            for (int i = 0; i < nt; i++)
            {
                Tjuv t = new Tjuv();
                Random r = new Random();
                int x = r.Next(0, StadWidth);
                int y = r.Next(0, StadHeight);
                while (stad[y, x] != 0)
                {
                    x = r.Next(0, StadWidth);
                    y = r.Next(0, StadHeight);
                }
                t.PositionX = x;
                t.PositionY = y;
                t.DirectionX = RandomDirection();
                t.DirectionY = RandomDirection();
                tjuvar.Add(t);
                stad[y, x] = (int)TypeCitizen.Thief;
            }
            for (int i = 0; i < nc; i++)
            {
                Manniska m = new Manniska();
                Random r = new Random();
                int x = r.Next(0, StadWidth);
                int y = r.Next(0, StadHeight);
                while (stad[y, x] != 0)
                {
                    x = r.Next(0, StadWidth);
                    y = r.Next(0, StadHeight);
                }
                m.PositionX = x;
                m.PositionY = y;
                m.DirectionX = RandomDirection();
                m.DirectionY = RandomDirection();
                m.Inventory.Add(new Objekt(m.PersonNummer, (int)TypeObjekt.Klocka));
                m.Inventory.Add(new Objekt(m.PersonNummer, (int)TypeObjekt.Mobiltelefon));
                m.Inventory.Add(new Objekt(m.PersonNummer, (int)TypeObjekt.Nycklar));
                m.Pengar = r.Next(0, 5000);
                manniskor.Add(m);
                stad[y, x] = (int)TypeCitizen.Person;
            }
        }


        // 
        private static int RandomDirection()
        {
            int ret = new Random().Next(0, 8);
            if (ret >= 0 && ret <= 2) ret = -1;
            else if (ret >= 3 && ret <= 5) ret = 0;
            else ret = 1;
            return ret;
        }


        private static void PrintIntro()
        {
            string intro = "\n\n" +
                "                     .@@@@@@@@@\n" +
                "                  @@@@@@@@@@ @@@@*\n" +
                "               @@@@@@@@@@      @@@@@\n" +
                "           @@@@@@@@@@@@@@@    @@@@@@@                                 @@@@@@@@@@%\n" +
                "         @@@@@@@@@@@@@@@@@@  @@@@@@@@                              @@@@@@@@@@@@@@@@@@\n" +
                "         @@@@@@@@@@@@@@@@@@@@@@@@@@@                             @@@@@@@@@@@@@@@@@@@@@@\n" +
                "           @@@@@@@@@@@@@@@@@@@@@@@@@                           .@@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "           @@@@@@@@@@@@@@@@@@@@@@@@@@@                           @@@@ @@@@@@ @@@@@@@@@@@\n" +
                "       @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                          @@@@ @@@@@@@@ @@@@@@@@@@\n" +
                "       @@@%  @@@@@@@@@@@  @@@@@  @@@                        (@@@@@@@@@@@@@@@@@@@@@@@@@&\n" +
                "              @@@@@@@@@@@@@@@@@@@@@                        &@@@@@@@@  @@@@@  @@@@@@@@@@@@@\n" +
                "               @@@@@@@@@@@@@@@@@@@@                            (@@@@@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "                 @@@@@@@@@@@@@@@@.          .,@@@@@@     @ @ @  &@@@@@@ @@@@@@@@@@@@@\n" +
                "                 @@@@@@@@@@@@@@           @@@@@@@@         @@@@@   @@@@@@@@@@@@@@@@      @ @ @\n" +
                "                @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@              @@@@@@@@@@@@@@@@@@@@@@    @@@@@@#\n" +
                "               @@@@@@@@@@@@@@@ #@@@@@@@@@@/@@,                      @@@@@@@@@@@@%*@@@@@@@@@\n" +
                "              @@@@@@@@@@@@@@@@                                      @@@@@@@@@@@@%\n" +
                "               @@@@@@@@@@@@@@@                                      *@@@@@@@@@@@\n" +
                "               @@@@@@@&     @@                                         @@@@@@@@\n" +
                "               @@@@@@@      @@@                                       @@*   @@\n" +
                "               @@@@@@@     @@@.                                      (@@    @@@\n" +
                "                  @@@@     @@@                                       @@@    @@@\n" +
                "                  @@@@    @@@@                                      (@@@    @@@\n" +
                "                  @@@@@   @@@@@                                    @@@@@   @@@@@&\n" +
                "\n" +
                "                                      POLISER OCH TJUVAR\n\n";
            Console.WriteLine(intro);
            Console.Write("Tryck på något tangent för att fortsätta...");
            Console.ReadKey(true);
        }
    }
}
