using System;

namespace GreatPyramidTreasureConsoleRPG
{
    public interface IEnemy
    {
        int Hp { get; set; }

        int MinDmg { get; set; }

        int MaxDmg { get; set; }

        int Exp { get; set; }

        int Gold { get; set; }

        string Name { get; set; }

        public void Attack(IClass characterClass)
        {
            if (characterClass != null)
            {
                int dmg = StandardFunctions.RandDmg(this.MinDmg, this.MaxDmg) - characterClass.Armor;
                if (dmg <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Nie otrzymałeś obrażeń!");
                    Console.ResetColor();
                }
                else
                {
                    characterClass.Hp -= dmg;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Otrzymałeś {dmg} obrażeń! Twoje punkty Hp {characterClass.Hp} / {characterClass.MaxHP}");
                    Console.ResetColor();
                }
            }
        }

        public bool IsAlive()
        {
            return this.Hp > 0;
        }
    }
}