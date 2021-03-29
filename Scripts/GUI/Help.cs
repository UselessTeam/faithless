using System.Collections.Generic;

public static class Help {
    public static string Get (string key) {
        return BB.Format(Explanations[key]);
    }
    public static readonly Dictionary<string, string> Explanations = new Dictionary<string, string>() {
{"help-battle",
@"Escape

[?battle]Help[/?]
[?retry]Give Up[/?]
"},
{"help-village",
@"Help

Seal the Yokai, earn Mon ([mon] coins), acquire new Talismans, and bright back peace
"},
{"retry",
@"Give Up
Are you sure you want to give up?

[url=~combat]Retry[/url]
[url=~village]Back to Village[/url]
[?help-battle]Cancel[/?]
"},
{"battle",
@"Help

Surround the Yokai by seals to seal it.
Hover a talisman to see its effect.
Select a talisman and a seal-slot to activate the effect.
"},
{"fire",
@"Fire Spread

At the beginning of your turn, each [wood-seal] adjacent to [fire-seal] will be turned into [fire-seal] and give one [ki]
"}};
}