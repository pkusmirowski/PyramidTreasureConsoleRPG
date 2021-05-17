using System.Collections.Generic;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class GenerateEnemies
    {
        public static void SelectOpponents(IClass characterClass, List<IEnemy> enemy)
        {
            if (characterClass != null && enemy != null)
            {
                if (characterClass.Level <= 3)
                {
                    EnemiesLevel0_3(enemy);
                }
                else if (characterClass.Level > 3 && characterClass.Level <= 6)
                {
                    EnemiesLevel3_6(enemy);
                }
                else if (characterClass.Level > 6 && characterClass.Level <= 9)
                {
                    EnemiesLevel6_9(enemy);
                }
                else if (characterClass.Level > 9 && characterClass.Level < 12)
                {
                    EnemiesLevel9_12(enemy);
                }
                else if (characterClass.Level > 12 && characterClass.Level < 15)
                {
                    EnemiesLevel12_15(enemy);
                }
                else if (characterClass.Level > 15 && characterClass.Level < 18)
                {
                    EnemiesLevel15_18(enemy);
                }
                else if (characterClass.Level > 18 && characterClass.Level < 20)
                {
                    EnemiesLevel18_20(enemy);
                }
            }
        }

        private static void EnemiesLevel0_3(List<IEnemy> enemy)
        {
            enemy.Add(new Thief());
            enemy.Add(new Thief());
        }

        private static void EnemiesLevel3_6(List<IEnemy> enemy)
        {
            enemy.Add(new Thief());
            enemy.Add(new Thief());
            enemy.Add(new Wolf());
            enemy.Add(new Wolf());
        }

        private static void EnemiesLevel6_9(List<IEnemy> enemy)
        {
            enemy.Add(new Thief());
            enemy.Add(new Wolf());
            enemy.Add(new Wolf());
            enemy.Add(new WildBoar());
        }

        private static void EnemiesLevel9_12(List<IEnemy> enemy)
        {
            enemy.Add(new Wolf());
            enemy.Add(new Wolf());
            enemy.Add(new WildBoar());
            enemy.Add(new WildBoar());
            enemy.Add(new ArmoredThief());
            enemy.Add(new ArmoredThief());
        }

        private static void EnemiesLevel12_15(List<IEnemy> enemy)
        {
            enemy.Add(new WildBoar());
            enemy.Add(new WildBoar());
            enemy.Add(new ArmoredThief());
            enemy.Add(new FallenKnight());
            enemy.Add(new FallenKnight());
        }

        private static void EnemiesLevel15_18(List<IEnemy> enemy)
        {
            enemy.Add(new Wolf());
            enemy.Add(new WildBoar());
            enemy.Add(new ArmoredThief());
            enemy.Add(new FallenKnight());
            enemy.Add(new FallenKnight());
            enemy.Add(new Templar());
            enemy.Add(new Templar());
        }

        private static void EnemiesLevel18_20(List<IEnemy> enemy)
        {
            enemy.Add(new FallenKnight());
            enemy.Add(new FallenKnight());
            enemy.Add(new Templar());
            enemy.Add(new Templar());
            enemy.Add(new CryingMonk());
        }
    }
}