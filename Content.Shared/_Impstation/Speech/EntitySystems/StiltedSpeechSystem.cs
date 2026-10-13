// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Text.RegularExpressions;
using Content.Shared.Speech.Components;
using Content.Shared._Impstation.Speech.Components;
using Content.Shared.Speech.EntitySystems; // starcup

namespace Content.Shared._Impstation.Speech.EntitySystems;
// starcup - moved to Content.Shared

public sealed partial class StiltedSpeechSystem : RelayAccentSystem<StiltedSpeechComponent>
{
    // @formatter:off
    private static readonly Regex RegexStartOfWord = new(@"(?:^|\s)([a-z])");
    // @formatter:on

    public override string Accentuate(string message, Entity<StiltedSpeechComponent>? ent = null) // starcup
    {
        var msg = // starcup
            // Capitalize the first letter of every word
            RegexStartOfWord.Replace(message, match => match.Value.ToUpper()); // starcup

        return msg; // starcup
    }
}
