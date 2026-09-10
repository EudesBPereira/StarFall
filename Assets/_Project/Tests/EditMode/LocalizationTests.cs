using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class LocalizationTests
    {
        [SetUp]
        public void SetUp()
        {
            Loc.Register(Language.Portuguese, PortugueseStrings.Table);
            Loc.Set(Language.English);
        }

        [TearDown]
        public void TearDown() => Loc.Set(Language.English);

        [Test]
        public void English_ReturnsSource()
        {
            Assert.That(Loc.T("PLAY"), Is.EqualTo("PLAY"));
            Assert.That(Loc.F("STAGE {0}", 3), Is.EqualTo("STAGE 3"));
        }

        [Test]
        public void Portuguese_TranslatesAndFallsBack()
        {
            Loc.Set(Language.Portuguese);
            Assert.That(Loc.T("PLAY"), Is.EqualTo("JOGAR"));
            Assert.That(Loc.F("STAGE {0}", 3), Is.EqualTo("FASE 3"));
            Assert.That(Loc.T("some untranslated text"), Is.EqualTo("some untranslated text"), "fallback to source");
            Assert.That(Loc.Has("PLAY"), Is.True);
            Assert.That(Loc.Has("nope"), Is.False);
        }

        [Test]
        public void Changed_FiresOnSwitch()
        {
            int fired = 0;
            void Handler() => fired++;
            Loc.Changed += Handler;
            Loc.Set(Language.Portuguese);
            Loc.Set(Language.Portuguese);
            Loc.Set(Language.English);
            Loc.Changed -= Handler;
            Assert.That(fired, Is.EqualTo(2), "no event when the language does not change");
        }

        [Test]
        public void SystemLanguage_Mapping()
        {
            Assert.That(Loc.FromSystem("Portuguese"), Is.EqualTo(Language.Portuguese));
            Assert.That(Loc.FromSystem("pt-BR"), Is.EqualTo(Language.Portuguese));
            Assert.That(Loc.FromSystem("English"), Is.EqualTo(Language.English));
            Assert.That(Loc.FromSystem(null), Is.EqualTo(Language.English));
            Assert.That(Loc.Next(Language.English), Is.EqualTo(Language.Portuguese));
        }

        [Test]
        public void PortugueseTable_PlaceholdersMatchSource()
        {
            foreach (var kv in PortugueseStrings.Table)
            {
                for (int i = 0; i < 5; i++)
                {
                    string ph = "{" + i + "}";
                    Assert.That(kv.Value.Contains(ph), Is.EqualTo(kv.Key.Contains(ph)), $"placeholder {ph} mismatch in '{kv.Key}'");
                }
                Assert.That(kv.Value, Is.Not.Empty, kv.Key);
            }
        }

        [Test]
        public void PortugueseTable_CoversLogicDisplayStrings()
        {
            Loc.Set(Language.Portuguese);
            for (int i = 0; i < UpgradeCatalog.NodeCount; i++)
            {
                Assert.That(Loc.Has(UpgradeCatalog.DisplayName((UpgradeNode)i)), Is.True, UpgradeCatalog.DisplayName((UpgradeNode)i));
                Assert.That(Loc.Has(UpgradeCatalog.BonusLabel((UpgradeNode)i)), Is.True, UpgradeCatalog.BonusLabel((UpgradeNode)i));
            }
            for (int i = 0; i < BuildMods.Count; i++)
            {
                Assert.That(Loc.Has(BuildMods.DisplayName((BuildModId)i)), Is.True, BuildMods.DisplayName((BuildModId)i));
                Assert.That(Loc.Has(BuildMods.Description((BuildModId)i)), Is.True, BuildMods.Description((BuildModId)i));
            }
            for (int i = 0; i < AchievementRules.Count; i++)
            {
                Assert.That(Loc.Has(AchievementRules.DisplayName((AchievementId)i)), Is.True);
                Assert.That(Loc.Has(AchievementRules.Description((AchievementId)i)), Is.True);
            }
            foreach (FactionId f in System.Enum.GetValues(typeof(FactionId)))
                Assert.That(Loc.Has(FactionRules.FactionName(f)), Is.True, FactionRules.FactionName(f));
            foreach (UltimateKind u in System.Enum.GetValues(typeof(UltimateKind)))
                Assert.That(Loc.Has(FactionRules.UltimateName(u)), Is.True, FactionRules.UltimateName(u));
        }
    }
}
