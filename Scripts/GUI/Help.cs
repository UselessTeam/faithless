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

[?save-quit]Save and quit[/?]
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
{"save-quit",
@"Would you like to save and quit the game?

[url=~title]Save and quit[/url]
[?help-village]Cancel[/?]
"},
{"ignite",
@"Ignition

At the beginning of your turn, each [wood-seal] adjacent to [fire-seal] will be turned into [fire-seal] and give one [ki]
"},
{"harvest",
@"Harvest

At the beginning of your turn, each [wood-seal] adjacent to [water-seal] will let you draw an additional talisman
"},
{"seed",
@"Seeds

[center][img=48]res://Assets/Sprites/GUI/icons/seeds.png[/img][/center]

Certain wood talismans give seeds as an effect.
Once you have obtained 4 of them, they are consumed and a [wood-seal] is placed in a random [empty-seal]
"},
{"stagger",
@"Staggered

Yokai will become staggered if they attack a [metal-seal].
If so, they will attack less their next turn
"}
};
}