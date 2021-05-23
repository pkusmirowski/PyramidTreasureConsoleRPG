using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class DataSave
    {
        public static void LoadGame(ref IClass characterClass, ClassState stateVariables)
        {
            if (stateVariables != null)
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                folder = Path.Combine(folder, "GreatPyramidTreasureRPG_DataSave");
                Directory.CreateDirectory(folder);
                string dataFile = Path.Combine(folder, "DataSave");
                if (File.Exists(dataFile))
                {
                    using Stream stateStream = File.OpenRead(dataFile);
                    BinaryFormatter deserializer = new();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                    stateVariables = (ClassState)deserializer.Deserialize(stateStream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                }

                var classType = stateVariables.ClassType;
                bool value = true;

                while (value)
                {
                    switch (classType)
                    {
                        case 1:
                            characterClass = new Warrior(stateVariables.Name);
                            ConvertStatsToClass(characterClass, stateVariables);
                            value = false;
                            break;

                        case 2:
                            characterClass = new Archer(stateVariables.Name);
                            ConvertStatsToClass(characterClass, stateVariables);
                            value = false;
                            break;

                        case 3:
                            characterClass = new Assassin(stateVariables.Name);
                            ConvertStatsToClass(characterClass, stateVariables);
                            value = false;
                            break;

                        default:
                            StandardFunctions.NoOption();
                            break;
                    }
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Gra została wczytana!\n");
                Console.ResetColor();
            }
        }

        public static void SaveGame(IClass characterClass, ClassState stateVariables)
        {
            Console.WriteLine("Czy na pewno chcesz zapisać gre? Nadpisze to aktualny plik: save!\n");
            Console.WriteLine("1. Nie");
            Console.WriteLine("2. Tak");
            int choice = StandardFunctions.ToInt32(Console.ReadLine());
            Console.Clear();
            if (choice == 2)
            {
                if (characterClass != null && stateVariables != null)
                {
                    ConvertStatsToSave(characterClass, stateVariables);
                    string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    folder = Path.Combine(folder, "GreatPyramidTreasureRPG_DataSave");
                    Directory.CreateDirectory(folder);
                    string dataFile = Path.Combine(folder, "DataSave");
                    using Stream stateStream = File.Create(dataFile);
                    BinaryFormatter serializer = new();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                    serializer.Serialize(stateStream, stateVariables);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Gra została zapisana!\n");
                Console.ResetColor();
            }
        }

        private static void ConvertStatsToSave(IClass characterClass, ClassState stateVariables)
        {
            stateVariables.Hp = characterClass.Hp;
            stateVariables.MaxHP = characterClass.MaxHP;
            stateVariables.Vit = characterClass.Vit;
            stateVariables.Str = characterClass.Str;
            stateVariables.Dex = characterClass.Dex;
            stateVariables.Exp = characterClass.Exp;
            stateVariables.MaxExp = characterClass.MaxExp;
            stateVariables.Level = characterClass.Level;
            stateVariables.Name = characterClass.Name;
            stateVariables.Gold = characterClass.Gold;
            stateVariables.MinDmg = characterClass.MinDmg;
            stateVariables.MaxDmg = characterClass.MaxDmg;
            stateVariables.Armor = characterClass.Armor;
            stateVariables.AttakChance = characterClass.AttakChance;
            stateVariables.CriticalAttackChance = characterClass.CriticalAttackChance;
            stateVariables.ClassType = characterClass.ClassType;
        }

        private static void ConvertStatsToClass(IClass characterClass, ClassState stateVariables)
        {
            characterClass.Hp = stateVariables.Hp;
            characterClass.MaxHP = stateVariables.MaxHP;
            characterClass.Vit = stateVariables.Vit;
            characterClass.Str = stateVariables.Str;
            characterClass.Dex = stateVariables.Dex;
            characterClass.Exp = stateVariables.Exp;
            characterClass.MaxExp = stateVariables.MaxExp;
            characterClass.Level = stateVariables.Level;
            characterClass.Name = stateVariables.Name;
            characterClass.Gold = stateVariables.Gold;
            characterClass.MinDmg = stateVariables.MinDmg;
            characterClass.MaxDmg = stateVariables.MaxDmg;
            characterClass.Armor = stateVariables.Armor;
            characterClass.AttakChance = stateVariables.AttakChance;
            characterClass.CriticalAttackChance = stateVariables.CriticalAttackChance;
        }
    }
}