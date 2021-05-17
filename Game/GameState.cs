using System;
using System.Collections.Generic;
using System.Linq;

namespace GreatPyramidTreasureConsoleRPG
{

    public class GameState
    {
        private readonly ClassState stateVariables = new ClassState();

        private List<IEnemy> enemy = new List<IEnemy>();

        private IClass characterClass;

        private bool game = true;

        public GameState()
        {
            Console.WriteLine("1: Nowa gra.");
            Console.WriteLine("2: Wczytaj gre.");
            int choice = StandardFunctions.ToInt32(Console.ReadLine());
            Console.Clear();
            switch (choice)
            {
                case 1:
                    Console.WriteLine("Podaj swoje imie: ");
                    var name = Console.ReadLine();
                    Console.Clear();
                    this.Name = name;
                    this.SelectClass();
                    break;

                case 2:
                    DataSave.LoadGame(ref this.characterClass, this.stateVariables);
                    break;

                default:
                    StandardFunctions.NoOption();
                    break;
            }
        }

        public string Name { get; set; }

        public void StartGame()
        {
            this.game = true;
            while (this.game)
            {
                if (this.characterClass.Level < 20)
                {
                    Console.WriteLine("Co chcesz zrobić:");
                    Console.WriteLine("1: Udaj się w droge.");
                    Console.WriteLine("2: Idź do tawerny.");
                    Console.WriteLine("3. Idź do sklepu.");
                    Console.WriteLine("4. Pokaż statystyki bochatera.");
                    Console.WriteLine("5. Ekwipunek.");
                    Console.WriteLine("6. Zapisz gre.");
                    Console.WriteLine("7. Zakończ gre.");
                    int choice = StandardFunctions.ToInt32(Console.ReadLine());
                    Console.Clear();
                    switch (choice)
                    {
                        case 1:
                            this.FightOpponents();
                            break;

                        case 2:
                            Tavern.TavernOptions(this.characterClass);
                            break;

                        case 3:

                            Shop.PotionShop(this.characterClass);
                            break;

                        case 4:
                            this.ShowStats();
                            break;

                        case 5:
                            UsingPotions.PotionOptions(this.characterClass);
                            break;

                        case 6:
                            DataSave.SaveGame(this.characterClass, this.stateVariables);
                            break;

                        case 7:
                            this.game = false;
                            break;

                        default:
                            StandardFunctions.NoOption();
                            break;
                    }
                }
                else if (this.characterClass.Level == 20)
                {
                    Console.WriteLine("Co chcesz zrobić:");
                    Console.WriteLine("1: Wyrusz z karawaną do piramid.");
                    Console.WriteLine("2: Idź do tawerny.");
                    Console.WriteLine("3. Pokaż statystyki bochatera.");
                    Console.WriteLine("4. Ekwipunek.");
                    Console.WriteLine("5. Zapisz gre.");
                    Console.WriteLine("6. Zakończ gre.");
                    int choice = StandardFunctions.ToInt32(Console.ReadLine());
                    Console.Clear();
                    switch (choice)
                    {
                        case 1:
                            this.FinalBoss();
                            break;

                        case 2:
                            Tavern.TavernOptions(this.characterClass);
                            break;

                        case 3:
                            this.ShowStats();
                            break;

                        case 4:
                            UsingPotions.PotionOptions(this.characterClass);
                            break;

                        case 5:
                            DataSave.SaveGame(this.characterClass, this.stateVariables);
                            break;

                        case 6:
                            this.game = false;
                            break;

                        default:
                            StandardFunctions.NoOption();
                            break;
                    }
                }
                else if (this.characterClass.Level > 20)
                {
                    Console.WriteLine("Wygrałeś! Możesz wyłączyć gre.");
                    this.game = false;
                }

                if (!this.characterClass.IsAlive())
                {
                    Console.WriteLine("Zostałeś pokonany.");
                    Console.WriteLine("Koniec gry.");
                    this.game = false;
                }
            }
        }

        private void FightOpponents()
        {
            GenerateEnemies.SelectOpponents(this.characterClass, this.enemy);
            this.enemy = this.enemy.OrderBy(_ => Guid.NewGuid()).ToList();
            this.Fight();
            this.enemy.Clear();
        }

        private void FinalBoss()
        {
            Dialogues.PyramidHistory();
            this.enemy.Add(new Anubis());
            this.enemy.Add(new Anubis());
            this.Fight();
            this.enemy.Clear();
            Dialogues.AfterGuards();
            this.characterClass.Hp = this.characterClass.MaxHP;
            this.enemy.Add(new Ra());
            this.Fight();
            this.enemy.Clear();
            this.characterClass.Level++;
            Dialogues.Ending();
            this.game = false;
        }

        private void Fight()
        {
            foreach (var enemy in new List<IEnemy>(this.enemy))
            {
                Console.WriteLine($"\nSpokałeś przeciwnika: {enemy.Name}. Przygotuj się do walki!!!");
                while (enemy.IsAlive())
                {
                    Console.WriteLine($"{enemy.Name} atakuje!");
                    enemy.Attack(this.characterClass);
                    this.characterClass.Attack(enemy);

                    if (!this.characterClass.IsAlive())
                    {
                        Console.WriteLine("Zostałeś pokonany!");
                        Console.WriteLine("Koniec gry!");
                        Environment.Exit(-1);
                    }
                    else if (!enemy.IsAlive())
                    {
                        Console.Clear();
                        Console.WriteLine("Pokonałeś przeciwnika!");
                        this.characterClass.Gold += enemy.Gold;
                        if (this.characterClass.Level < 20)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.WriteLine($"Zyskałeś {enemy.Exp} punktów doświadczenia!");
                            Console.WriteLine($"Zdobyłeś {enemy.Gold} sztuk złota!");
                            Console.ResetColor();
                            this.characterClass.Exp += enemy.Exp;
                            this.characterClass.AddLevel();
                        }

                        this.enemy.Remove(enemy);
                    }
                }
            }
        }

        private void SelectClass()
        {
            while (this.game)
            {
                Console.WriteLine("Wybierz klasę:");
                Console.WriteLine("1: Wojownik.    Poziom trudności: Łatwy");
                Console.WriteLine("2: Łucznik.     Poziom trudności: Średni");
                Console.WriteLine("3. Skrytobójca. Poziom trudności: Trudny");
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();
                switch (choice)
                {
                    case 1:
                        this.characterClass = new Warrior(this.Name);
                        this.game = false;
                        break;

                    case 2:
                        this.characterClass = new Archer(this.Name);
                        this.game = false;
                        break;

                    case 3:
                        this.characterClass = new Assassin(this.Name);
                        this.game = false;
                        break;

                    default:
                        StandardFunctions.NoOption();
                        break;
                }
            }
        }

        private void ShowStats()
        {
            Console.WriteLine("-------------------");
            Console.WriteLine($"Imię: {this.characterClass.Name}");
            Console.WriteLine($"Punkty zdrowia: {this.characterClass.Hp}/{this.characterClass.MaxHP}");
            if (this.characterClass.Level < 20)
            {
                Console.WriteLine($"Poziom: {this.characterClass.Level}");
            }
            else
            {
                Console.WriteLine($"Masz maksymalny poziom: {this.characterClass.Level}");
            }

            Console.WriteLine($"Punkty doświadczenia: {this.characterClass.Exp}/{this.characterClass.MaxExp}");
            Console.WriteLine($"Atak: {this.characterClass.MinDmg} - {this.characterClass.MaxDmg}");
            Console.WriteLine($"Pancerz: {this.characterClass.Armor}");
            Console.WriteLine($"Złoto: {this.characterClass.Gold}");
            Console.WriteLine("-------------------");
        }
    }
}