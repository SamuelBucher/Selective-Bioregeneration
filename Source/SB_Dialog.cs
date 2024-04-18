//#define RIMWORLD_1_4

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Selective_Bioregeneration
{
    public class SB_Dialog_HediffSelection : Window
    {
        private readonly string Title1;
        private readonly string Title2;
        private readonly List<Hediff> MaybeHeal;
        private readonly List<Hediff> WillHeal;
        private Hediff SelectedHediff;
        private readonly ITargetedHealingCycle Cycle;

        private readonly Action GiveJobAct;

        private Vector2 scrollPosition = Vector2.zero;
        public override Vector2 InitialSize => new Vector2(550f, 550f);

        private static readonly string[] labels = { "SelectiveBioregeneration.Part", "SelectiveBioregeneration.HealthCondition", "SelectiveBioregeneration.Severity" };
        private static readonly float[] mults = { 2f, 3f, 1f };

        public static void CreateDialog(ITargetedHealingCycle cycle, Pawn pawn, List<Hediff> maybeHeal, List<Hediff> willHeal, Action/*<Hediff>*/ giveJobAct)
        {
            if (pawn == null)
            {
                Log.Error($"Attempted creating {nameof(SB_Dialog_HediffSelection)} with null pawn");
                return;
            }

            if (maybeHeal?.Count > 0 || willHeal?.Count > 0)
            {
                maybeHeal.SortByDescending((hediff) => HealthCardUtility.GetListPriority(hediff.Part));
                Find.WindowStack.Add(new SB_Dialog_HediffSelection(cycle, pawn, maybeHeal, willHeal, giveJobAct));
            }
            else
                Messages.Message("SelectiveBioregeneration.NoHediffsToHeal".Translate(pawn), MessageTypeDefOf.RejectInput, false);
        }

        private SB_Dialog_HediffSelection(ITargetedHealingCycle cycle, Pawn pawn, List<Hediff> maybeHeal, List<Hediff> willHeal, Action/*<Hediff>*/ execute)
        {
            Title1 = "SelectiveBioregeneration.DialogTitle1".Translate(pawn);
            Title2 = "SelectiveBioregeneration.DialogTitle2".Translate();

            /*if (!(maybeHeal?.Count > 0))
                Log.Error($"{nameof(SB_Dialog_HediffSelection)} created with empty Hediff list (is null: {maybeHeal == null})");*/
            MaybeHeal = maybeHeal ?? new List<Hediff>();
            WillHeal = willHeal ?? new List<Hediff>();
            SelectedHediff = null;
            Cycle = cycle;

            GiveJobAct = execute;

            forcePause = true;
            absorbInputAroundWindow = true;
            onlyOneOfTypeAllowed = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var oriColor = GUI.color;
            var oriFont = Text.Font;

            float y = inRect.y;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, y, inRect.width, 42f), Title1);
            y += 28f;

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, y, inRect.width, 42f), Title2);
            y += 28f;

            float width = inRect.width - 16f;
            float dialogTextHeight = Text.CalcHeight("foo", width);
            Rect rect = new Rect(10f, y, width - 10f, dialogTextHeight);
            float x = rect.x;
            float w = 0;
            for (int i = 0; i < labels.Length; i++)
            {
                w = GetLabelWidth(rect.width, mults[i]);
                Widgets.Label(
                new Rect(x, rect.y, w, rect.height),
                labels[i].Translate());
                x += w + 4f;
            }
            y += dialogTextHeight;

            GUI.color = Widgets.SeparatorLineColor;
            Widgets.DrawLineHorizontal(0f, y, width);
            GUI.color = oriColor;

            Rect outRect = new Rect(inRect.x, y, inRect.width, inRect.height - 35f - 5f - y);
            Rect viewRect = new Rect(0f, 0f, width, CalcOptionsHeight(width));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            var totalHeight = 0f;
            y = 0;
            foreach (var hediff in MaybeHeal)
            {
                var hediffHeight = CalcHediffHeight(hediff, width);
                rect = new Rect(
                    10f,
                    y + 4f,
                    viewRect.width - 10f,
                    hediffHeight);

                if (Mouse.IsOver(rect))
                    Widgets.DrawHighlight(rect);

                if (RadioButtonHediff(rect, hediff, SelectedHediff == hediff))
                    SelectedHediff = hediff;

                y += hediffHeight + 8f;
                totalHeight += hediffHeight + 8f;
            }
            foreach (var hediff in WillHeal)
            {
                var hediffHeight = CalcHediffHeight(hediff, width);
                rect = new Rect(
                    10f,
                    y + 4f,
                    viewRect.width - 10f,
                    hediffHeight);

                if (Mouse.IsOver(rect))
                    Widgets.DrawHighlight(rect);

                NoButtonHediff(rect, hediff);

                y += hediffHeight + 8f;
                totalHeight += hediffHeight + 8f;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "CancelButton".Translate(), doMouseoverSound: false))
                Close();

            if ((SelectedHediff != null || MaybeHeal?.Count == 0)
                && Widgets.ButtonText(new Rect(inRect.width / 2f + 20f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "Confirm".Translate(), doMouseoverSound: false))
            {
                Close();
                if (SelectedHediff != null)
                    Cycle.TargetHediff = SelectedHediff;
                GiveJobAct.Invoke();
            }

            Text.Font = oriFont;
        }

        private bool RadioButtonHediff(Rect rect, Hediff hediff, bool chosen)
        {
            var oriColor = GUI.color;
            var pawn = hediff.pawn;

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            var x = rect.x;

            // Part
            if (hediff.Part == null)
                GUI.color = Color.red;
            else
                GUI.color = HealthUtility.GetPartConditionLabel(pawn, hediff.Part).second;
            var width = GetLabelWidth(rect.width, mults[0]);
            Widgets.Label(
                new Rect(x, rect.y, width, rect.height),
                hediff.Part?.LabelCap ?? "WholeBody".Translate());
            x += width + 4f;

            // Condition
            GUI.color = hediff.LabelColor;
            width = GetLabelWidth(rect.width, mults[1]);
            Widgets.Label(
                new Rect(x, rect.y, width, rect.height),
                hediff.LabelCap);
            x += width + 4f;

            // Severity
            width = GetLabelWidth(rect.width, mults[2]);
            Widgets.Label(
                new Rect(x, rect.y, width, rect.height),
                hediff.SeverityLabel);

            Text.Anchor = anchor;
            GUI.color = oriColor;

            bool num = Widgets.ButtonInvisible(rect);
            if (num && !chosen)
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();

            #if RIMWORLD_1_4

            Widgets.RadioButtonDraw(
                rect.x + rect.width - 24f,
                rect.y + rect.height / 2f - 12f,
                chosen);
            return num;

            #else

            Widgets.RadioButtonDraw(
                rect.x + rect.width - 24f,
                rect.y + rect.height / 2f - 12f,
                chosen, false);
            return num;
            
            #endif
        }

        private void NoButtonHediff(Rect rect, Hediff hediff)
        {
            var oriColor = GUI.color;
            var pawn = hediff.pawn;

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            var x = rect.x;

            // Part
            if (hediff.Part == null)
                GUI.color = Color.red;
            else
                GUI.color = HealthUtility.GetPartConditionLabel(pawn, hediff.Part).second;
            var width = GetLabelWidth(rect.width, mults[0]);
            Widgets.Label(
                new Rect(x, rect.y, width, rect.height),
                hediff.Part?.LabelCap ?? "WholeBody".Translate());
            x += width + 4f;

            // Condition
            GUI.color = hediff.LabelColor;
            width = GetLabelWidth(rect.width, mults[1]);
            Widgets.Label(
                new Rect(x, rect.y, width, rect.height),
                hediff.LabelCap);
            x += width + 4f;

            // Severity
            width = GetLabelWidth(rect.width, mults[2]);
            Widgets.Label(
                new Rect(x, rect.y, width, rect.height),
                hediff.SeverityLabel);

            Text.Anchor = anchor;
            GUI.color = oriColor;
        }

        private float CalcOptionsHeight(float width)
        {
            float height = 0f;
            foreach (var hediff in MaybeHeal)
                height += CalcHediffHeight(hediff, width) + 8f;
            foreach (var hediff in WillHeal)
                height += CalcHediffHeight(hediff, width) + 8f;
            return height;
        }

        private float CalcHediffHeight(Hediff hediff, float width) =>
            Mathf.Max(Text.CalcHeight(hediff.Part?.Label, GetLabelWidth(width, mults[0])), Text.CalcHeight(hediff.Label, GetLabelWidth(width, mults[1])));

        private float GetLabelWidth(float width, float mult) =>
            (width - 24f) * (mult / 6f) - 4f;


        private static bool IsValidHediff(Pawn pawn, Hediff hediff) =>
            hediff.Visible
            && hediff.def.isBad
            && hediff.def.everCurableByItem
            && !hediff.FullyImmune()
            && !(hediff is Hediff_MissingPart
                && hediff.Part != null
                && (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(hediff.Part) || (hediff.Part.parent != null && pawn.health.hediffSet.PartIsMissing(hediff.Part.parent))));
    }
}
