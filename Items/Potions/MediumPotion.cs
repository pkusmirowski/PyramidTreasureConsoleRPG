namespace GreatPyramidTreasureConsoleRPG
{
    public class MediumPotion : IItem
    {
        public MediumPotion()
        {
            this.Id = 2;
            this.Name = "Średnia mikstura lecząca";
            this.Price = 50;
            this.RestoreHP = 50;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public int RestoreHP { get; set; }
    }
}