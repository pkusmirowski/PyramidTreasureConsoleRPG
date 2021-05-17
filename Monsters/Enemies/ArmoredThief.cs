namespace GreatPyramidTreasureConsoleRPG
{
    public class ArmoredThief : IEnemy
    {
        public ArmoredThief()
        {
            this.Hp = 80;
            this.MinDmg = 23;
            this.MaxDmg = 31;
            this.Exp = 2000;
            this.Gold = 20;
            this.Name = "Opancerzony złodziej";
        }

        public int Hp { get; set; }

        public int MinDmg { get; set; }

        public int MaxDmg { get; set; }

        public int Exp { get; set; }

        public int Gold { get; set; }

        public string Name { get; set; }
    }
}