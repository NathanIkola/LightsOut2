using System;
using UnityEngine;
using Verse;

namespace jl08lib.Settings.Extensions
{
    /// <summary>
    /// Extensions for the setting listing
    /// </summary>
    public static class Listing_StandardExtensions
    {
        /// <summary>
        /// Draws a numeric text field in the setting listing with a left-aligned label
        /// </summary>
        /// <typeparam name="TNumeric">The number type</typeparam>
        /// <param name="listing">The listing instance to add to</param>
        /// <param name="label">The label to show</param>
        /// <param name="value">The value being modified</param>
        /// <param name="buffer">The buffer to store the string in</param>
        /// <param name="tooltip">The tooltip for it</param>
        public static void TextFieldNumericLabelled<TNumeric>(this Listing_Standard listing, string label, ref TNumeric value, ref string buffer, string tooltip)
            where TNumeric : struct
        {
            float height = Text.CalcHeight(label, listing.ColumnWidth);
            Rect rect = listing.GetRect(height, 1f);
            rect.width = Math.Min(rect.width + 24f, listing.ColumnWidth);
            if (listing.BoundingRectCached is null || rect.Overlaps(listing.BoundingRectCached.Value))
            {
                Rect rect2 = rect.LeftHalf().Rounded();
                Rect rect3 = rect.RightHalf().Rounded();
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect2, label);
                Text.Anchor = anchor;
                Widgets.TextFieldNumeric(rect3, ref value, ref buffer);
                if (!string.IsNullOrWhiteSpace(tooltip))
                {
                    if (Mouse.IsOver(rect))
                    {
                        Widgets.DrawHighlight(rect);
                    }
                    TooltipHandler.TipRegion(rect, tooltip);
                }
            }
        }
    }
}