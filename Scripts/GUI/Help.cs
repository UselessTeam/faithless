using System.Collections.Generic;

public static class Help {
    public static string Get (string key) {
        return BB.Format(Explanations[key]);
    }
    public static readonly Dictionary<string, string> Explanations = new Dictionary<string, string>() {
{"esc",
@"Escape

[?retry]Give Up[/?]
"},
{"retry",
@"Give Up
Are you sure you want to give up?

[url=~combat]Retry[/url]
[url=~village]Back to Village[/url]
[?esc]Cancel[/?]
"},
{"fire",
@"Fire Spread

At the beginning of your turn, each [wood-seal] adjacent to [fire-seal] will be turned into [fire-seal] and give one [ki]
"}};
}