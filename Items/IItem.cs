namespace GreatPyramidTreasureConsoleRPG
{
    public interface IItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public int RestoreHP { get; set; }
    }
}