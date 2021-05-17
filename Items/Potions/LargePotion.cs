namespace GreatPyramidTreasureConsoleRPG
{
    public class LargePotion : IItem
    {
        public LargePotion()
        {
            this.Id = 3;
            this.Name = "Duża mikstura lecząca";
            this.Price = 100;
            this.RestoreHP = 100;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public int RestoreHP { get; set; }
    }
}