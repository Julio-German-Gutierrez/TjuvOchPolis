using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace TjuvOchPolis
{
    class Program
    {
        // Bi-dimensional array representing the stad.
        // It takes its values from StadWidth and StadHeight.
        static int[,] city;
        static int CityWidth { get { return 100; } } // 80
        static int CityHeight { get { return 30; } } // 24

        // Lists of characters.
        static List<Policeman> policemen = new List<Policeman>();
        static List<Thief> thieves = new List<Thief>();
        static List<Person> people = new List<Person>();
        static int amountPolicemen { get { return 10; } } // 10
        static int amountThieves { get { return 20; } } // 20
        static int amountPeople { get { return 30; } } // 30

        // "policeReports" is a collection of messages about the encounters.
        //  Like for ex. "Tjuv rånar medborgare" and so...
        // "location" is a list of coordinates of the encounters.
        static List<string> policeReports = new List<string>();
        static List<int[]> location = new List<int[]>();
        static List<Thief> prison = new List<Thief>();

        // incidents is TRUE when something happens in the city.
        static bool incidents = false;

        static Timer timer; // Main timer called every second.
        static Timer pause; // Timer used to pause X seconds to read the messages.
        static Stopwatch stopwatch; // Stopwatch to calculate the time in prison.

        static int reclusionTime = 20000; // 20 seconds in miliseconds.
        static int pauseTime = 3000; // Time to pause messages.
        static int frameTime = 1000; // Main timer time.

        static void Main(string[] args)
        {
            // We set Buffer size and Window's console size.
            Console.SetBufferSize(CityWidth + 1, CityHeight + 15);
            Console.SetWindowSize(CityWidth + 1, CityHeight + 15);
            Console.BackgroundColor = ConsoleColor.Black;

            IntroScreen();

            // ClearStad() Fills the stad[,] with zeros.
            // Zeros are interpreted as empty spaces by the PrintStad() method.
            ClearStadArray();

            // CreateCharacters() create the characters
            // and fill the lists with them.
            CreateCharacters(amountPolicemen, amountThieves, amountPeople);

            // Start timer to check prison time.
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // Set main call event
            timer = new Timer(frameTime); // Called every 1 second or 1000 miliseconds.
            timer.AutoReset = true;
            timer.Elapsed += MainCall; // Method to be called every 1 second.
            timer.Start();

            Console.ReadLine(); // Simple exit system. Press any key to exit.
        }


        // Method called every second by the main Timer.
        private static void MainCall(object sender, ElapsedEventArgs e)
        {
            MoveStad();

            CheckAndHandleIncidents();

            PrintStad();
        }

        // Method to clear stad[,]
        private static void ClearStadArray()
        {
            city = new int[CityHeight, CityWidth];
            for (int i = 0; i < city.GetLength(0); i++)
            {
                for (int j = 0; j < city.GetLength(1); j++)
                {
                    city[i, j] = 0;
                }
            }
        }


        private static void CreateCharacters(int numPolis, int numTjuv, int numMann)
        {
            for (int i = 0; i < numPolis; i++)
            {
                Policeman p = new Policeman();
                SetCharacter(p);
                policemen.Add(p);
            }
            for (int i = 0; i < numTjuv; i++)
            {
                Thief t = new Thief();
                SetCharacter(t);
                thieves.Add(t);
            }
            for (int i = 0; i < numMann; i++)
            {
                Person m = new Person();
                SetCharacter(m);
                people.Add(m);
            }
        }// End CreateCharacters()


        private static void SetCharacter(Citizen m)
        {
            Random r = new Random();
            int x = r.Next(0, CityWidth);
            int y = r.Next(0, CityHeight);
            while (city[y, x] != 0)
            {
                x = r.Next(0, CityWidth);
                y = r.Next(0, CityHeight);
            }
            m.PositionX = x;
            m.PositionY = y;
            m.DirectionX = r.Next(-1, 2); // posible vectors -1 / 0 / 1
            m.DirectionY = r.Next(-1, 2);

            if (m is Person)
            {
                m.inventory.Add(new Objekt(m.PersonNummer, (int)TypeObjekt.Watch));
                m.inventory.Add(new Objekt(m.PersonNummer, (int)TypeObjekt.SmartPhone));
                m.inventory.Add(new Objekt(m.PersonNummer, (int)TypeObjekt.Keys));
                m.Money = r.Next(0, 5000);
                city[y, x] = (int)TypeCitizen.Person;
            }
            else if (m is Policeman) city[y, x] = (int)TypeCitizen.Police;
            else city[y, x] = (int)TypeCitizen.Thief; // if (m is Tjuv)
        }


        private static void MoveStad()
        {
            ClearStadArray(); // Empty the stad by filling it with zeros.

            foreach (Person m in people) { MoveCharacter(m); }

            foreach (Thief t in thieves) { MoveCharacter(t); }

            foreach (Policeman p in policemen) { MoveCharacter(p); }
        }// End of StepForwardStad()

        private static void MoveCharacter(Citizen m)
        {
            m.PositionX += m.DirectionX;
            if (m.PositionX > CityWidth - 1) m.PositionX = 0;
            if (m.PositionX < 0) m.PositionX = CityWidth - 1;

            m.PositionY += m.DirectionY;
            if (m.PositionY > CityHeight - 1) m.PositionY = 0;
            if (m.PositionY < 0) m.PositionY = CityHeight - 1;

            if (m is Person)
            {
                WriteStadArray(m.PositionX, m.PositionY, (int)TypeCitizen.Person);
            }
            else if (m is Thief)
            {
                WriteStadArray(m.PositionX, m.PositionY, (int)TypeCitizen.Thief);
            }
            else // if(m is Polis)
            {
                WriteStadArray(m.PositionX, m.PositionY, (int)TypeCitizen.Police);
            }

        }


        // The sole purpose of WriteToStadArray() is to be able to naturally
        // write de coordinates as X follow by Y, instead of having 
        // to invert it to Y follow by X. Because in stad[,] I interpret
        // the first dimension as a column. Like Y.
        private static void WriteStadArray(int x, int y, int type)
        {
            city[y, x] = type; // Here the assign is inverted.
        }

        private static void CheckAndHandleIncidents()
        {
            foreach (Thief thief in thieves)
            {
                foreach (Person person in people)
                {
                    if (thief.PositionX == person.PositionX && thief.PositionY == person.PositionY && person.inventory.Count != 0)
                    {
                        if (!incidents) incidents = true;

                        // The thief steals one random object by using the Tjuv.StealFrom() method.
                        string objectStolen = thief.StealFrom(person);
                        location.Add(new int[] { thief.PositionX, thief.PositionY, (int)TypeCitizen.ThiefReport }); // GPS. Coordinates of the incident.
                        policeReports.Add($"Tjuven rånade {objectStolen} från medborgare med PID: {person.PersonNummer}");
                    }
                }
            }

            List<Thief> busToPrison = new List<Thief>(); // Because we can not modify a collection while looping in it.
            foreach (Policeman officer in policemen)
            {
                foreach (Thief criminal in thieves)
                {
                    if (officer.PositionX == criminal.PositionX && officer.PositionY == criminal.PositionY)
                    {
                        if (!incidents) incidents = true;

                        officer.RecoverEverythingFrom(criminal);
                        busToPrison.Add(criminal); // On his way to the Hole! 

                        location.Add(new int[] { officer.PositionX, officer.PositionY, (int)TypeCitizen.PoliceReport });
                        policeReports.Add($"Polisen fångade en tjuv!!!");
                    }
                }
            }

            if (busToPrison.Count != 0)
            {
                foreach (Thief criminal in busToPrison)
                {
                    criminal.StartTimeInPrison(stopwatch.ElapsedMilliseconds); // The stopwatch tell us when this scumbag came to prison.
                    thieves.Remove(criminal); // Out of the streets.
                    prison.Add(criminal); // To the hole. That'll teach him!
                }
                busToPrison.Clear(); // Empty the bus after all the criminals were transported.
            }


            // Now is time to check the prisoners for a parole.
            List<Thief> prisonersToReleased = new List<Thief>();
            if (prison.Count != 0)
            {
                foreach (Thief criminal in prison)
                {
                    long timeServed = stopwatch.ElapsedMilliseconds - criminal.startTimeInPrison;
                    if (timeServed > 20000) // if time in prison is more than 20 seconds. Too little if you ask me...
                    {
                        if (!incidents) incidents = true;
                        prisonersToReleased.Add(criminal);
                    }
                }
            }


            // We prepare to release the reformed criminal.
            if (prisonersToReleased.Count != 0)
            {
                foreach (Thief criminal in prisonersToReleased)
                {
                    prison.Remove(criminal);
                    thieves.Add(criminal); // Back to the streets.
                    policeReports.Add("En tjuv FRIGAVS från fängelset. Watch OUT!!!");
                    location.Add(new int[] { criminal.PositionX, criminal.PositionY, (int)TypeCitizen.ThiefReport });
                }
                prisonersToReleased.Clear();
            }
        } // end of CheckCollitions()


        // Draw in console.
        private static void PrintStad()
        {
            Console.Clear();

            if (incidents)
            {
                // We clear everything to show just the places
                // where the incidents took place.
                ClearStadArray();

                foreach (int[] i in location)
                {
                    if (i[2] == 4) city[i[1], i[0]] = (int)TypeCitizen.ThiefReport; // Tjuv
                    else city[i[1], i[0]] = (int)TypeCitizen.PoliceReport; // Polis
                }
            }

            // We paint the screen with information from stad[,] array.
            for (int i = 0; i < city.GetLength(0); i++)
            {
                for (int j = 0; j < city.GetLength(1); j++)
                {
                    switch (city[i, j])
                    {
                        case 0:
                            {
                                Console.Write(" ");
                                break;
                            }
                        case 1:
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write("P");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            }
                        case 2:
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("T");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            }
                        case 3:
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write("M");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            }
                        case 4:
                            {
                                Console.BackgroundColor = ConsoleColor.Red;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write("T");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.BackgroundColor = ConsoleColor.Black;
                                break;
                            }
                        case 5:
                            {
                                Console.BackgroundColor = ConsoleColor.Blue;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write("P");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.BackgroundColor = ConsoleColor.Black;
                                break;
                            }
                        default:
                            break;
                    }
                }
                Console.WriteLine(); // WriteLine after each line in stad.
            }


            //Console.WriteLine($"Antal tjuvar: {tjuvar.Count}/{numberThieves}                                    Antal människor: {manniskor.Count}/{numberCitizens}");
            Console.WriteLine("{0,15}{1:00}/{2:00}{3,37}{4:00}{5,35}{6:00}/{7:00}", "Antal tjuvar: ", thieves.Count, amountThieves, "Tjuvar i fängelset: ", prison.Count, "Antal poliser: ", policemen.Count, amountPolicemen);

            // We inform the population about the time in prison of the criminals.
            if (prison.Count != 0)
            {
                foreach (Thief criminal in prison)
                {
                    long tiempoActual = stopwatch.ElapsedMilliseconds;
                    long tiempoRestante = reclusionTime - (tiempoActual - criminal.startTimeInPrison);
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"Tjuven {criminal.PersonNummer} kommer frisläppas om: {tiempoRestante / 1000} sekunder.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }

            // Daily reports of incidents. Those who contains the words "polis", "frisläppas" and "watch",
            // are police reports and as such are painted in blue colors.
            // The rest in red.
            foreach (string s in policeReports)
            {
                if (s.ToLower().Contains("polis") || s.ToLower().Contains("frisläppas") || s.ToLower().Contains("watch"))
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


            // If collitions were detected, the simulation is stopped
            // for the selected interval "pauseTime", giving time to read the collitions.
            // Otherwise we continue with the 1 sec interval MainCall() event.
            if (incidents)
            {
                timer.Stop();
                stopwatch.Stop();
                pause = new Timer(pauseTime);
                pause.Elapsed += PauseToReport;
                pause.AutoReset = false;
                pause.Start();
            }
            else
            {
                timer.Start(); // No incidents reported? Let's restart the timer -> "MainCall()"
            }

            // Clear the buffers.
            incidents = false;
            policeReports.Clear();
            location.Clear();

        }// End of PrintStad()

        private static void PauseToReport(object sender, ElapsedEventArgs e)
        {
            timer.Start();
            stopwatch.Start();
            pause.Dispose();

        }

        private static void IntroScreen()
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Clear();
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
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ReadKey(true);
        }
    }
}
