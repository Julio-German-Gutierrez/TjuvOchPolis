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
                case (int)TypeObjekt.Keys:
                    {
                        returnType = "Nycklar";
                        break;
                    }
                case (int)TypeObjekt.SmartPhone:
                    {
                        returnType = "Mobiltelefon";
                        break;
                    }
                case (int)TypeObjekt.Watch:
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
        Keys = 0,
        SmartPhone = 1,
        Watch = 2
    }

    enum TypeCitizen
    {
        Police = 1,
        Thief = 2,
        Person = 3,
        ThiefReport = 4,
        PoliceReport = 5
    }
}
