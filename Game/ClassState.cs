using System;
namespace GreatPyramidTreasureConsoleRPG
{
    [Serializable]
    public class ClassState
    {
        public int Hp { get; set; }

        public int MaxHP { get; set; }

        public int Vit { get; set; }

        public int Str { get; set; }

        public int Dex { get; set; }

        public double Exp { get; set; }

        public double MaxExp { get; set; }

        public int Level { get; set; }

        public string Name { get; set; }

        public int Gold { get; set; }

        public int MinDmg { get; set; }

        public int MaxDmg { get; set; }

        public int Armor { get; set; }

        public double AttakChance { get; set; }

        public double CriticalAttackChance { get; set; }

        public int ClassType { get; set; }
    }
}