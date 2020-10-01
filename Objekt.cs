namespace TjuvOchPolis
{
    class Objekt
    {
        public int Owner { get; set; }
        public int Type { get; set; }

        public Objekt(int owner, int type)
        {
            Owner = owner;
            Type = type;
        }

        public override string ToString()
        {
            string returnType = "";
            switch (Type)
            {
                case (int)TypeObjekt.Nycklar:
                    {
                        returnType = "Nycklar";
                        break;
                    }
                case (int)TypeObjekt.Mobiltelefon:
                    {
                        returnType = "Mobiltelefon";
                        break;
                    }
                case (int)TypeObjekt.Klocka:
                    {
                        returnType = "Klocka";
                        break;
                    }
                default:
                    break;
            }
            return returnType;
        }
    }

    enum TypeObjekt
    {
        Nycklar = 0,
        Mobiltelefon = 1,
        Klocka = 2
    }

    enum TypeCitizen
    {
        Polis = 1,
        Thief = 2,
        Person = 3
    }
}
