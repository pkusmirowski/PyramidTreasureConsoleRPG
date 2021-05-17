namespace GreatPyramidTreasureConsoleRPG
{
    public class Templar : IEnemy
    {
        public Templar()
        {
            this.Hp = 120;
            this.MinDmg = 45;
            this.MaxDmg = 57;
            this.Exp = 3500;
            this.Gold = 40;
            this.Name = "Templariusz";
        }

        public int Hp { get; set; }

        public int MinDmg { get; set; }

        public int MaxDmg { get; set; }

        public int Exp { get; set; }

        public int Gold { get; set; }

        public string Name { get; set; }
    }
}