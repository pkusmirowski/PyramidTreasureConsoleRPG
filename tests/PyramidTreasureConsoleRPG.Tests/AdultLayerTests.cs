namespace PyramidTreasureConsoleRPG.Tests;

public class AdultLayerTests
{
    [Fact]
    public void Debt_AccruesInterest_AndBecomesOverdueAfterGrace()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.Gold = 0;
        Assert.Equal(LoanOutcome.Granted, DebtService.Borrow(hero, 100));
        Assert.Equal(100, hero.Gold);
        Assert.Equal(100, hero.Debt);
        Assert.Equal(LoanOutcome.AlreadyInDebt, DebtService.Borrow(hero, 50));

        DayReport report = DayService.AdvanceDays(hero, 1);
        Assert.Equal(110, hero.Debt);
        Assert.False(hero.HasFlag("debt:overdue"));
        Assert.False(hero.HasFlag("debt:canpay"));

        DayService.AdvanceDays(hero, DayService.DebtGraceDays - 1);
        Assert.True(hero.HasFlag("debt:overdue"));
        Assert.True(hero.Debt > 150);

        report = DayService.AdvanceDays(hero, 1);
        // Flaga nakładana raz – nie ma powtórnego komunikatu.
        Assert.DoesNotContain(report.Notes, n => n.Contains("egzekutorów", StringComparison.Ordinal));
    }

    [Fact]
    public void Debt_TooLargeLoanIsRefused_AndRepaymentClearsFlags()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        Assert.Equal(LoanOutcome.TooMuch, DebtService.Borrow(hero, DayService.MaxLoan + 1));
        Assert.Equal(RepayOutcome.NoDebt, DebtService.Repay(hero, 10));

        DebtService.Borrow(hero, 200);
        hero.Gold = 500;
        DayService.AdvanceDays(hero, DayService.DebtGraceDays);
        Assert.True(hero.HasFlag("debt:overdue"));
        Assert.True(hero.HasFlag("debt:canpay"));

        Assert.Equal(RepayOutcome.NotEnoughGold, DebtService.Repay(hero, 10_000));
        Assert.Equal(RepayOutcome.Partial, DebtService.Repay(hero, 100));
        Assert.Equal(RepayOutcome.Repaid, DebtService.Repay(hero, hero.Debt));
        Assert.Equal(0, hero.Debt);
        Assert.False(hero.HasFlag("debt:overdue"));
        Assert.False(hero.HasFlag("debt:canpay"));
    }

    [Fact]
    public void OverdueDebt_ForcesCollectorsEvent_AndPayingOnTheSpotNeedsGold()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.Gold = 0;
        DebtService.Borrow(hero, 100);
        DayService.AdvanceDays(hero, DayService.DebtGraceDays);
        Assert.True(hero.HasFlag("debt:overdue"));

        GameEvent? picked = EventEngine.Pick(hero, RegionId.Port, new SeededRandomSource(3));
        Assert.NotNull(picked);
        Assert.True(picked.Forced);
        Assert.Equal("debt_collectors_port", picked.Id);

        // Bez złota nie można spłacić na miejscu.
        Assert.False(EventEngine.Availability(hero, picked.Choices[1]).Available);

        hero.Gold = 10_000;
        DayService.AdvanceDays(hero, 1);
        Assert.True(EventEngine.Availability(hero, picked.Choices[1]).Available);
        int debt = hero.Debt;
        EventResult result = EventEngine.Resolve(hero, picked, 1, new SeededRandomSource(1));
        Assert.True(result.Success);
        Assert.Equal(10_000 - debt, hero.Gold);
        Assert.Equal(0, hero.Debt);
        Assert.False(hero.HasFlag("debt:overdue"));
        GameEvent? afterwards = EventEngine.Pick(hero, RegionId.Port, new SeededRandomSource(3));
        Assert.True(afterwards is null || !afterwards.Forced);
    }

    [Fact]
    public void Lotus_BuffsNextFight_ThenAddictionCravingAndWithdrawal()
    {
        Hero hero = Hero.Create(HeroClass.Archer, "Test");
        double baseDamage = hero.DamageMultiplier;
        double baseHit = hero.HitChance;

        for (int i = 0; i < DayService.AddictionThreshold; i++)
        {
            hero.AddPotion(Potion.Lotus);
            Assert.Equal(0, hero.DrinkPotion(PotionKind.Lotus));
        }

        Assert.Equal(PotionKind.Lotus, hero.NextFightBuff);
        Assert.Equal(DayService.AddictionThreshold, hero.Addiction);
        hero.ApplyNextFightBuff();
        Assert.Null(hero.NextFightBuff);
        Assert.True(hero.DamageMultiplier > baseDamage);
        hero.ClearCombatState();
        Assert.Equal(baseDamage, hero.DamageMultiplier);

        DayReport report = DayService.AdvanceDays(hero, DayService.CravingAfterDays);
        Assert.True(hero.Craving);
        Assert.Contains(report.Notes, n => n.Contains("Głód", StringComparison.Ordinal));
        Assert.True(hero.DamageMultiplier < baseDamage);
        Assert.True(hero.HitChance < baseHit);

        // Kolejna dawka gasi głód i odkłada odstawienie.
        hero.AddPotion(Potion.Lotus);
        hero.DrinkPotion(PotionKind.Lotus);
        Assert.False(hero.Craving);
        Assert.Equal(hero.Day, hero.LastLotusDay);

        DayService.AdvanceDays(hero, DayService.WithdrawalDays);
        Assert.Equal(0, hero.Addiction);
        Assert.False(hero.Craving);
    }

    [Fact]
    public void Whisky_TradesEvasionForHit_AndAntidoteCuresPoisonAndCraving()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Test");
        int baseEvasion = hero.Evasion;
        double baseHit = hero.HitChance;

        hero.AddPotion(Potion.Whisky);
        hero.DrinkPotion(PotionKind.Whisky);
        hero.ApplyNextFightBuff();
        Assert.True(hero.HitChance > baseHit);
        Assert.Equal(Math.Max(0, baseEvasion - 10), hero.Evasion);
        hero.ClearCombatState();
        Assert.Equal(baseEvasion, hero.Evasion);

        hero.Statuses.Add(StatusKind.Poison, turns: 3, power: 5);
        hero.Craving = true;
        hero.AddPotion(Potion.Antidote);
        hero.DrinkPotion(PotionKind.Antidote);
        Assert.False(hero.Statuses.Has(StatusKind.Poison));
        Assert.False(hero.Craving);
    }

    [Fact]
    public void Nightmares_LimitRestToNinetyPercent_ForThreeNights()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.Gold = 1000;
        hero.NightmareNights = 1;
        hero.TakeDamage(hero.MaxHp / 2);

        Assert.Equal(RestOutcome.Nightmares, RestService.RentRoom(hero));
        Assert.Equal(hero.MaxHp * 9 / 10, hero.Hp);
        Assert.Equal(0, hero.NightmareNights);

        Assert.Equal(RestOutcome.Ok, RestService.RentRoom(hero));
        Assert.Equal(hero.MaxHp, hero.Hp);
    }

    [Fact]
    public void Interrogation_SuccessGivesIntelAndNightmares_FailureOnlyCosts()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test"); // Str 4
        PrisonerResult failed = InterrogationService.Resolve(hero, PrisonerChoice.Interrogate, RegionId.Delta, new ScriptedRandomSource(5));
        Assert.Contains("porażka", failed.Notes[0], StringComparison.Ordinal);
        Assert.False(hero.HasFlag("intel:Delta"));
        Assert.Equal(3, hero.NightmareNights);
        Assert.Equal(-8, hero.GetReputation(Faction.Town));
        Assert.Equal(5, hero.GetReputation(Faction.Underworld));

        int exp = hero.Exp;
        PrisonerResult passed = InterrogationService.Resolve(hero, PrisonerChoice.Interrogate, RegionId.Delta, new ScriptedRandomSource(20));
        Assert.Contains("sukces", passed.Notes[0], StringComparison.Ordinal);
        Assert.True(hero.HasFlag("intel:Delta"));
        Assert.True(hero.Exp > exp || hero.Level > 1);
    }

    [Fact]
    public void Prisoner_ExecuteAndRelease_ShiftReputation()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        InterrogationService.Resolve(hero, PrisonerChoice.Execute, RegionId.Port, new SeededRandomSource(1));
        Assert.Equal(-3, hero.GetReputation(Faction.Town));
        Assert.Equal(3, hero.GetReputation(Faction.Underworld));
        InterrogationService.Resolve(hero, PrisonerChoice.Release, RegionId.Port, new SeededRandomSource(1));
        Assert.Equal(2, hero.GetReputation(Faction.Town));
        Assert.Equal(0, hero.NightmareNights);
    }

    [Fact]
    public void PrisonerSurvives_OnlyAfterHumans_NeverAfterBosses()
    {
        var always = new ScriptedRandomSource(1, 1, 1, 1);
        Assert.True(InterrogationService.PrisonerSurvives([EnemyCatalog.Thief], always));
        Assert.False(InterrogationService.PrisonerSurvives([EnemyCatalog.Wolf], always));
        Assert.False(InterrogationService.PrisonerSurvives([EnemyCatalog.Anubis], always));
        Assert.False(InterrogationService.PrisonerSurvives([EnemyCatalog.Thief], new ScriptedRandomSource(100)));
    }

    [Fact]
    public void Dialogue_EntryFollowsFlags_AndOptionsGateOnFlagsGoldAndReputation()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        NpcDefinition zoja = NpcCatalog.Zoja;
        DialogueNode start = DialogueEngine.Start(hero, zoja);
        Assert.Equal("zoja_intro", start.Id);

        DialogueResult result = DialogueEngine.Choose(hero, zoja, start, 0);
        Assert.True(hero.HasFlag("zoja:met"));
        Assert.NotNull(result.Next);
        Assert.Equal("zoja_intro_2", result.Next.Id);

        DialogueNode revenge = DialogueEngine.Start(hero, zoja);
        Assert.Equal("zoja_revenge", revenge.Id);
        Assert.False(DialogueEngine.Availability(hero, revenge.Options[0]).Available);
        Assert.Throws<InvalidOperationException>(() => DialogueEngine.Choose(hero, zoja, revenge, 0));

        hero.SetFlag("quest:CaptainSmugglers");
        Assert.True(DialogueEngine.Availability(hero, revenge.Options[0]).Available);
        DialogueEngine.Choose(hero, zoja, revenge, 0);
        Assert.True(hero.HasFlag("zoja:revenge"));
        Assert.Equal("zoja_night", DialogueEngine.Start(hero, zoja).Id);

        // Ptaszek: opcja za złoto.
        hero.Gold = 0;
        DialogueNode ptaszek = DialogueEngine.Start(hero, NpcCatalog.Ptaszek);
        DialogueOption paid = ptaszek.Options.First(o => o.GoldCost > 0);
        Assert.False(DialogueEngine.Availability(hero, paid).Available);
        hero.Gold = paid.GoldCost;
        DialogueEngine.Choose(hero, NpcCatalog.Ptaszek, ptaszek, ptaszek.Options.ToList().IndexOf(paid));
        Assert.Equal(0, hero.Gold);

        // Neferet: ślub wymaga reputacji Bractwa.
        hero.SetFlag("neferet:wine");
        DialogueNode vow = DialogueEngine.Start(hero, NpcCatalog.Neferet);
        Assert.Equal("neferet_vow", vow.Id);
        DialogueOption stay = vow.Options.First(o => o.RequiresFaction == Faction.Brotherhood);
        Assert.False(DialogueEngine.Availability(hero, stay).Available);
        hero.AdjustReputation(Faction.Brotherhood, 10);
        Assert.True(DialogueEngine.Availability(hero, stay).Available);
    }

    [Fact]
    public void NpcCatalog_IsConsistent()
    {
        Assert.Equal(3, NpcCatalog.All.Count);
        foreach (NpcDefinition npc in NpcCatalog.All)
        {
            Assert.NotEmpty(npc.Entry);
            Assert.Null(npc.Entry[^1].RequiresFlag);
            Assert.Equal(npc.Nodes.Count, npc.Nodes.Select(n => n.Id).Distinct().Count());
            foreach ((_, _, string nodeId) in npc.Entry)
            {
                Assert.Contains(npc.Nodes, n => n.Id == nodeId);
            }

            foreach (DialogueNode node in npc.Nodes)
            {
                Assert.NotEmpty(node.Text);
                Assert.NotEmpty(node.Options);
                foreach (DialogueOption option in node.Options)
                {
                    Assert.False(string.IsNullOrWhiteSpace(option.Text));
                    if (option.Next is not null)
                    {
                        Assert.Contains(npc.Nodes, n => n.Id == option.Next);
                    }
                }
            }

            // Z każdym NPC da się skończyć wątek: świeży bohater z pełnym portfelem i reputacją dochodzi do końca.
            Hero hero = Hero.Create(HeroClass.Warrior, "Test");
            hero.Gold = 10_000;
            hero.SetFlag("quest:CaptainSmugglers");
            foreach (Faction f in Enum.GetValues<Faction>())
            {
                hero.AdjustReputation(f, 100);
            }

            string? previous = null;
            for (int round = 0; round < 12; round++)
            {
                DialogueNode node = DialogueEngine.Start(hero, npc);
                if (node.Id == previous)
                {
                    break;
                }

                previous = node.Id;
                while (true)
                {
                    int index = node.Options.ToList().FindIndex(o => DialogueEngine.Availability(hero, o).Available);
                    Assert.True(index >= 0, $"{npc.Name}/{node.Id}: brak dostępnej opcji");
                    DialogueResult r = DialogueEngine.Choose(hero, npc, node, index);
                    if (r.Next is null)
                    {
                        break;
                    }

                    node = r.Next;
                }
            }
        }
    }

    [Fact]
    public void Save_RoundTripsAdultState()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Test");
        DebtService.Borrow(hero, 300);
        hero.Addiction = 2;
        hero.LastLotusDay = 4;
        hero.Craving = true;
        hero.NightmareNights = 2;
        hero.NextFightBuff = PotionKind.Whisky;
        hero.SetFlag("zoja:met");

        SaveData data = hero.ToSaveData();
        Assert.Equal(SaveData.CurrentVersion, data.Version);
        Hero loaded = Hero.FromSaveData(data);

        Assert.Equal(300, loaded.Debt);
        Assert.Equal(hero.DebtDay, loaded.DebtDay);
        Assert.Equal(2, loaded.Addiction);
        Assert.Equal(4, loaded.LastLotusDay);
        Assert.True(loaded.Craving);
        Assert.Equal(2, loaded.NightmareNights);
        Assert.Equal(PotionKind.Whisky, loaded.NextFightBuff);
        Assert.True(loaded.HasFlag("zoja:met"));
    }

    [Fact]
    public void Profanity_OnlyWhenEnabled()
    {
        var harshRng = new ScriptedRandomSource(1, 0, 1, 0, 1, 0);
        var mildRng = new ScriptedRandomSource(1, 0, 1, 0, 1, 0);
        string harsh = Dialogues.NoGold(harshRng, harsh: true);
        string mild = Dialogues.NoGold(mildRng, harsh: false);
        Assert.False(string.IsNullOrWhiteSpace(harsh));
        Assert.False(string.IsNullOrWhiteSpace(mild));
        Assert.NotEqual(harsh, mild);
    }
}
