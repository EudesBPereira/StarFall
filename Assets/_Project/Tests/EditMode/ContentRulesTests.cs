using System.Collections.Generic;
using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class StageVariationTests
    {
        [Test]
        public void SameInputs_SamePick()
        {
            Assert.That(StageVariation.PickVariant(42, 3, 7, 4), Is.EqualTo(StageVariation.PickVariant(42, 3, 7, 4)));
            Assert.That(StageVariation.Roll(42, 3, 7, 0.5f), Is.EqualTo(StageVariation.Roll(42, 3, 7, 0.5f)));
        }

        [Test]
        public void DifferentEvents_UseDifferentStreams()
        {
            int same = 0;
            for (int e = 0; e < 50; e++)
                if (StageVariation.PickVariant(7, 0, e, 10) == StageVariation.PickVariant(7, 0, e + 1, 10)) same++;
            Assert.That(same, Is.LessThan(25), "consecutive events should not repeat picks most of the time");
        }

        [Test]
        public void Pick_StaysInRange_AndHandlesEdges()
        {
            for (int s = 0; s < 200; s++)
                Assert.That(StageVariation.PickVariant(s, 1, 2, 3), Is.InRange(0, 2));
            Assert.That(StageVariation.PickVariant(1, 1, 1, 0), Is.EqualTo(-1));
            Assert.That(StageVariation.PickVariant(1, 1, 1, 1), Is.EqualTo(0));
            Assert.That(StageVariation.Roll(1, 1, 1, 1f), Is.True);
            Assert.That(StageVariation.Roll(1, 1, 1, 0f), Is.False);
            Assert.That(StageVariation.Value(1, 1, 1), Is.InRange(0f, 1f));
        }

        [Test]
        public void Roll_RespectsChanceStatistically()
        {
            int hits = 0;
            for (int s = 0; s < 1000; s++) if (StageVariation.Roll(s, 2, 5, 0.3f)) hits++;
            Assert.That(hits, Is.InRange(220, 380));
        }
    }

    public class BuildModsTests
    {
        [Test]
        public void Draft_IsDeterministic_DistinctAndSized()
        {
            var a = BuildMods.Draft(11, 0, 0, null);
            var b = BuildMods.Draft(11, 0, 0, null);
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.Count, Is.EqualTo(BuildMods.DraftSize));
            Assert.That(new HashSet<BuildModId>(a).Count, Is.EqualTo(a.Count), "no duplicates in one draft");
        }

        [Test]
        public void Draft_ExcludesExhaustedMods()
        {
            var chosen = new List<BuildModId> { BuildModId.Magnet };
            for (int d = 0; d < 30; d++)
                Assert.That(BuildMods.Draft(5, 1, d, chosen), Has.No.Member(BuildModId.Magnet), "Magnet cannot stack");
            var maxed = new List<BuildModId>();
            for (int i = 0; i < BuildMods.MaxStacks; i++) maxed.Add(BuildModId.HeavyRounds);
            for (int d = 0; d < 30; d++)
                Assert.That(BuildMods.Draft(5, 1, d, maxed), Has.No.Member(BuildModId.HeavyRounds));
        }

        [Test]
        public void Draft_ShrinksWhenPoolExhausted()
        {
            var all = new List<BuildModId>();
            for (int i = 0; i < BuildMods.Count; i++)
                for (int s = 0; s < BuildMods.StackLimit((BuildModId)i); s++) all.Add((BuildModId)i);
            Assert.That(BuildMods.Draft(1, 0, 0, all).Count, Is.EqualTo(0));
        }

        [Test]
        public void Compute_StacksMultiplicatively_AndCapsLifesteal()
        {
            var chosen = new List<BuildModId> { BuildModId.HeavyRounds, BuildModId.HeavyRounds, BuildModId.PiercingTips, BuildModId.WideSpread, BuildModId.Magnet };
            var m = BuildMods.Compute(chosen);
            Assert.That(m.Damage, Is.EqualTo(1.15f * 1.15f).Within(0.001f));
            Assert.That(m.Pierce, Is.EqualTo(1));
            Assert.That(m.ExtraShots, Is.EqualTo(1));
            Assert.That(m.Magnet, Is.True);
            Assert.That(m.FireRate, Is.EqualTo(1f));

            var heal = new List<BuildModId>();
            for (int i = 0; i < 10; i++) heal.Add(BuildModId.NanoRepair);
            Assert.That(BuildMods.Compute(heal).LifestealPerKill, Is.LessThanOrEqualTo(FactionRules.LifestealCap));
        }

        [Test]
        public void EveryMod_HasNameAndDescription()
        {
            for (int i = 0; i < BuildMods.Count; i++)
            {
                Assert.That(BuildMods.DisplayName((BuildModId)i), Is.Not.Empty);
                Assert.That(BuildMods.Description((BuildModId)i), Is.Not.Empty);
            }
            Assert.That(BuildMods.Count, Is.EqualTo(System.Enum.GetValues(typeof(BuildModId)).Length));
        }
    }
}
