using System;
using System.Collections.Generic;

namespace TjuvOchPolis
{
    class Medborgare
    {
        public List<Objekt> Inventory;
        public int Pengar { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int DirectionX { get; set; }
        public int DirectionY { get; set; }

        public Medborgare()
        {
            Inventory = new List<Objekt>();
        }

        static public int GetRandomID()
        {
            Random r = new Random();
            return r.Next(1, 10000);
        }
    }


    class Tjuv : Medborgare
    {
        Random r = new Random();
        public Tjuv()
        {
            Inventory = new List<Objekt>();
            Pengar = 0;
        }
        public string StealFrom(Medborgare m)
        {
            string stolen = "";
            if (m.Inventory.Count != 0)
            {
                int indexToSteal = r.Next(0, m.Inventory.Count);
                Objekt objectStolen = m.Inventory[indexToSteal];
                stolen = objectStolen.ToString();
                m.Inventory.Remove(objectStolen);
            }
            return stolen;
        }
    }


    class Polis : Medborgare
    {
        public Polis()
        {
            Inventory = new List<Objekt>();
        }

        public void RecoverEverythingFrom(Medborgare m)
        {
            if (m.Inventory.Count != 0)
            {
                Inventory.AddRange(m.Inventory);
                m.Inventory.Clear();
            }
        }
    }


    class Manniska : Medborgare
    {
        public int PersonNummer { get; set; }
        public Manniska()
        {
            PersonNummer = Medborgare.GetRandomID();
            Inventory = new List<Objekt>();
        }
    }
}
