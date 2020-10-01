using System;
using System.Collections.Generic;

namespace TjuvOchPolis
{
    class Citizen
    {
        public List<Objekt> inventory;
        public int Money { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int DirectionX { get; set; }
        public int DirectionY { get; set; }
        public int PersonNummer { get; set; }

        public Citizen()
        {
            inventory = new List<Objekt>();
        }

        static public int GetRandomID()
        {
            Random r = new Random();
            return r.Next(1, 10000);
        }
    }


    class Thief : Citizen
    {
        //List<Tjuv> prison;
        //List<Tjuv> free;
        public long startTimeInPrison;

        Random r = new Random();
        public Thief()
        {
            PersonNummer = Citizen.GetRandomID();
            inventory = new List<Objekt>();
            Money = 0;
        }
        public string StealFrom(Citizen m)
        {
            string stolen = "";
            if (m.inventory.Count != 0)
            {
                int indexToSteal = r.Next(0, m.inventory.Count);
                Objekt objectStolen = m.inventory[indexToSteal];
                stolen = objectStolen.ToString();
                m.inventory.Remove(objectStolen);
            }
            return stolen;
        }

        internal void StartTimeInPrison(long elapsedMilliseconds)
        {
            startTimeInPrison = elapsedMilliseconds;
        }
    }


    class Policeman : Citizen
    {
        public Policeman()
        {
            inventory = new List<Objekt>();
        }

        public void RecoverEverythingFrom(Citizen m)
        {
            if (m.inventory.Count != 0)
            {
                inventory.AddRange(m.inventory);
                m.inventory.Clear();
            }
        }
    }


    class Person : Citizen
    {

        public Person()
        {
            PersonNummer = Citizen.GetRandomID();
            inventory = new List<Objekt>();
        }
    }
}
