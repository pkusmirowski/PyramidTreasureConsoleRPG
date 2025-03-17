using System;
using System.Collections.Generic;
using System.Linq;

namespace GreatPyramidTreasureConsoleRPG
{
    public class GameState
    {
        private readonly ClassState stateVariables = new();
        private List<IEnemy> enemies = new();
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
                    StartNewGame();
                    break;
                case 2:
                    LoadGame();
                    break;
                default:
                    StandardFunctions.NoOption();
                    break;
            }
        }

        public string Name { get; set; }

        public void StartGame()
        {
            game = true;

            while (game)
            {
                if (characterClass.Level < 20)
                {
                    DisplayOptions();
                    HandleChoice(StandardFunctions.ToInt32(Console.ReadLine()));
                }
                else if (characterClass.Level == 20)
                {
                    DisplayFinalOptions();
                    HandleFinalChoice(StandardFunctions.ToInt32(Console.ReadLine()));
                }
                else if (characterClass.Level > 20)
                {
                    Console.WriteLine("Wygrałeś! Możesz wyłączyć grę.");
                    game = false;
                }

                if (!characterClass.IsAlive())
                {
                    Console.WriteLine("Zostałeś pokonany.");
                    Console.WriteLine("Koniec gry.");
                    game = false;
                }
            }
        }

        private void StartNewGame()
        {
            Dialogues.Intro();
            Console.WriteLine("Podaj swoje imie: ");
            Name = Console.ReadLine();
            Console.Clear();
            SelectClass();
        }

        private void LoadGame()
        {
            DataSave.LoadGame(ref characterClass);
        }

        private void DisplayOptions()
        {
            Console.WriteLine("Co chcesz zrobić:");
            Console.WriteLine("1: Udaj się w drogę.");
            Console.WriteLine("2: Idź do tawerny.");
            Console.WriteLine("3: Idź do sklepu.");
            Console.WriteLine("4: Pokaż statystyki bohatera.");
            Console.WriteLine("5: Ekwipunek.");
            Console.WriteLine("6: Zapisz grę.");
            Console.WriteLine("7: Zakończ grę.");
        }

        private void DisplayFinalOptions()
        {
            Console.WriteLine("Co chcesz zrobić:");
            Console.WriteLine("1: Wyrusz z karawaną do piramid.");
            Console.WriteLine("2: Idź do tawerny.");
            Console.WriteLine("3: Pokaż statystyki bohatera.");
            Console.WriteLine("4: Ekwipunek.");
            Console.WriteLine("5: Zapisz grę.");
            Console.WriteLine("6: Zakończ grę.");
        }

        private void HandleChoice(int choice)
        {
            Console.Clear();
            switch (choice)
            {
                case 1:
                    if (GameStatus.CheckGameStatus(characterClass))
                        FightOpponents();
                    break;
                case 2:
                    Tavern.TavernOptions(characterClass);
                    break;
                case 3:
                    Shop.PotionShop(characterClass);
                    break;
                case 4:
                    ShowStats();
                    break;
                case 5:
                    UsingPotions.PotionOptions(characterClass);
                    break;
                case 6:
                    DataSave.SaveGame(characterClass, stateVariables);
                    break;
                case 7:
                    game = false;
                    break;
                default:
                    StandardFunctions.NoOption();
                    break;
            }
        }

        private void HandleFinalChoice(int choice)
        {
            Console.Clear();
            switch (choice)
            {
                case 1:
                    FinalBoss();
                    break;
                case 2:
                    Tavern.TavernOptions(characterClass);
                    break;
                case 3:
                    ShowStats();
                    break;
                case 4:
                    UsingPotions.PotionOptions(characterClass);
                    break;
                case 5:
                    DataSave.SaveGame(characterClass, stateVariables);
                    break;
                case 6:
                    game = false;
                    break;
                default:
                    StandardFunctions.NoOption();
                    break;
            }
        }

        private void FightOpponents()
        {
            GenerateEnemies.SelectOpponents(characterClass, enemies);
            enemies = enemies.OrderBy(_ => Guid.NewGuid()).ToList();
            Fight();
            enemies.Clear();
        }

        private void FinalBoss()
        {
            Dialogues.PyramidHistory();
            enemies.Add(new Anubis());
            enemies.Add(new Anubis());
            Fight();
            enemies.Clear();
            Dialogues.AfterGuards();
            characterClass.Hp = characterClass.MaxHP;
            enemies.Add(new Ra());
            Fight();
            enemies.Clear();
            characterClass.Level++;
            Dialogues.Ending();
            game = false;
        }

        private void Fight()
        {
            foreach (var enemy in new List<IEnemy>(enemies))
            {
                Console.WriteLine($"\nSpokałeś przeciwnika: {enemy.Name}. Przygotuj się do walki!!!");
                StandardFunctions.Sleep();
                while (enemy.IsAlive())
                {
                    Console.WriteLine($"{enemy.Name} atakuje!");
                    enemy.Attack(characterClass);
                    characterClass.Attack(enemy);

                    if (!characterClass.IsAlive())
                    {
                        Console.WriteLine("Zostałeś pokonany!");
                        Console.WriteLine("Koniec gry!");
                        Environment.Exit(-1);
                    }
                    else if (!enemy.IsAlive())
                    {
                        Console.Clear();
                        Console.WriteLine("Pokonałeś przeciwnika!");
                        characterClass.Gold += enemy.Gold;
                        if (characterClass.Level < 20)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.WriteLine($"Zyskałeś {enemy.Exp} punktów doświadczenia!");
                            Console.WriteLine($"Zdobyłeś {enemy.Gold} sztuk złota!");
                            Console.ResetColor();
                            characterClass.Exp += enemy.Exp;
                            characterClass.AddLevel();
                        }

                        enemies.Remove(enemy);
                    }
                }
            }
        }

        private void SelectClass()
        {
            while (game)
            {
                Console.WriteLine("Wybierz klasę:");
                Console.WriteLine("1: Wojownik.");
                Console.WriteLine("2: Łucznik.");
                Console.WriteLine("3. Asasyn.");
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();
                switch (choice)
                {
                    case 1:
                        characterClass = new Warrior(Name);
                        game = false;
                        break;
                    case 2:
                        characterClass = new Archer(Name);
                        game = false;
                        break;
                    case 3:
                        characterClass = new Assassin(Name);
                        game = false;
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
            Console.WriteLine($"Imię: {characterClass.Name}");
            Console.WriteLine($"Punkty zdrowia: {characterClass.Hp}/{characterClass.MaxHP}");
            Console.WriteLine($"Poziom: {characterClass.Level}");

            if (characterClass.Level < 20)
            {
                Console.WriteLine($"Punkty doświadczenia: {characterClass.Exp}/{characterClass.MaxExp}");
            }
            else
            {
                Console.WriteLine($"Masz maksymalny poziom: {characterClass.Level}");
            }

            Console.WriteLine($"Atak: {characterClass.MinDmg} - {characterClass.MaxDmg}");
            Console.WriteLine($"Pancerz: {characterClass.Armor}");
            Console.WriteLine($"Złoto: {characterClass.Gold}");
            Console.WriteLine("-------------------");
        }
    }
}
