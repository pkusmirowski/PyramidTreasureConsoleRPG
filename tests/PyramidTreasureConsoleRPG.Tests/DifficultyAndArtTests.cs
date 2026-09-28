namespace PyramidTreasureConsoleRPG.Tests;

public class DifficultyAndArtTests
{
    [Fact]
    public void DifficultyCatalog_IsConsistent()
    {
        Assert.Equal(Enum.GetValues<Difficulty>().Length, DifficultyCatalog.All.Count);
        Assert.All(DifficultyCatalog.All, d => Assert.Same(d, DifficultyCatalog.Get(d.Kind)));
        Assert.True(DifficultyCatalog.Easy.EnemyPercent < DifficultyCatalog.Normal.EnemyPercent);
        Assert.True(DifficultyCatalog.Normal.EnemyPercent < DifficultyCatalog.Hard.EnemyPercent);
        Assert.True(DifficultyCatalog.Easy.RewardPercent > DifficultyCatalog.Hard.RewardPercent);
        Assert.Equal(100, DifficultyCatalog.Normal.EnemyPercent);
        Assert.Equal(100, DifficultyCatalog.Normal.RewardPercent);
    }

    [Fact]
    public void Difficulty_ScalesEnemiesAndRewards()
    {
        Hero easy = Hero.Create(HeroClass.Warrior, "E", Difficulty.Easy);
        Hero hard = Hero.Create(HeroClass.Warrior, "H", Difficulty.Hard);
        Assert.Equal(80, easy.EnemyScalePercent);
        Assert.Equal(125, hard.EnemyScalePercent);
        Assert.Equal(Difficulty.Normal, Hero.Create(HeroClass.Archer, "N").Difficulty);

        var engine = new CombatEngine(hard, [EnemyCatalog.Thief.Spawn()], new SeededRandomSource(4));
        Assert.Equal(EnemyCatalog.Thief.MaxHp * 125 / 100, engine.Enemies[0].MaxHp);
        engine.Begin();
        while (engine.Status == CombatStatus.InProgress)
        {
            engine.HeroAttack(AttackKind.Normal, engine.Alive[0]);
        }

        EnemyDefeatedEvent? defeated = null;
        var engine2 = new CombatEngine(easy, [EnemyCatalog.Thief.Spawn()], new SeededRandomSource(4));
        engine2.Begin();
        while (engine2.Status == CombatStatus.InProgress)
        {
            defeated ??= engine2.HeroAttack(AttackKind.Normal, engine2.Alive[0]).OfType<EnemyDefeatedEvent>().FirstOrDefault();
        }

        if (defeated is not null)
        {
            Assert.Equal(EnemyCatalog.Thief.Exp * 125 / 100, defeated.Exp);
            Assert.Equal(EnemyCatalog.Thief.Gold * 125 / 100, defeated.Gold);
        }
    }

    [Fact]
    public void Difficulty_SurvivesSaveAndNewGamePlus()
    {
        Hero hard = Hero.Create(HeroClass.Assassin, "H", Difficulty.Hard);
        Hero loaded = Hero.FromSaveData(hard.ToSaveData());
        Assert.Equal(Difficulty.Hard, loaded.Difficulty);
        Assert.Equal(7, SaveData.CurrentVersion);

        Hero old = Hero.FromSaveData(new SaveData { Name = "Stary", Class = 1, Version = 6, Vit = 10 });
        Assert.Equal(Difficulty.Normal, old.Difficulty);

        hard.SetFlag("map:nomads");
        EndingEngine.Apply(hard, EndingKind.Destroy);
        Hero plus = Hero.NewGamePlusFrom(hard);
        Assert.Equal(Difficulty.Hard, plus.Difficulty);
        Assert.Equal(125 * 130 / 100, plus.EnemyScalePercent);
    }

    [Fact]
    public void ArtCatalog_IsConsistent_AndMapsEveryEnemyClassAndRegion()
    {
        Assert.True(ArtCatalog.All.Count >= 15);
        foreach (AsciiArt art in ArtCatalog.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(art.Name));
            Assert.True(art.Lines.Count >= 4, art.Name);
            Assert.True(art.Width <= 60, $"{art.Name} jest za szeroki na cmd ({art.Width})");
            Assert.All(art.Lines, line => Assert.DoesNotContain('\t', line.Text));
        }

        Assert.All(Enum.GetValues<HeroClass>(), c => Assert.NotNull(ArtCatalog.ForClass(c)));
        Assert.All(Enum.GetValues<RegionId>(), r => Assert.NotNull(ArtCatalog.ForRegion(r)));
        Assert.All(EnemyCatalog.All, e => Assert.NotNull(ArtCatalog.ForEnemy(e)));
        Assert.Same(ArtCatalog.Ra, ArtCatalog.ForEnemy(EnemyCatalog.Ra));
        Assert.Same(ArtCatalog.Anubis, ArtCatalog.ForEnemy(EnemyCatalog.ShadowAnubis));
        Assert.Same(ArtCatalog.Beast, ArtCatalog.ForEnemy(EnemyCatalog.Wolf));
        Assert.Same(ArtCatalog.Human, ArtCatalog.ForEnemy(EnemyCatalog.Thief));
    }

    [Fact]
    public void Screens_ShowArt_AndRespectTheToggle()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        var io = new ScriptedGameIO("1", "1", "1", "1", "1", "1", "1", "1", "1", "1", "1", "1");
        new CombatScreen(io, new SeededRandomSource(1)).Run(hero, [EnemyCatalog.Wolf.Spawn()]);
        Assert.Contains("Bestia", io.ArtShown);

        var io2 = new ScriptedGameIO("1");
        new EndingScreen(io2).Run(Hero.Create(HeroClass.Archer, "Test"));
        Assert.Contains("Graal", io2.ArtShown);

        var plain = new ConsoleGameIO { ArtEnabled = false };
        plain.ShowArt(ArtCatalog.Title); // nie rzuca i nic nie pisze
        Assert.False(plain.ArtEnabled);
    }
}
