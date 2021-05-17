namespace GreatPyramidTreasureConsoleRPG
{
    public class SmallPotion : IItem
    {
        public SmallPotion()
        {
            this.Id = 1;
            this.Name = "Mała mikstura lecząca";
            this.Price = 20;
            this.RestoreHP = 20;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public int RestoreHP { get; set; }
    }
}