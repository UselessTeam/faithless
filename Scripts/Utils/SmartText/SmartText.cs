using System;
using Godot;

namespace Utils {
    public class SmartText : RichTextLabel {
        public override void _Ready () {
            Connect("meta_clicked", this, nameof(MetaClicked));
        }

        private void MetaClicked (string info) {
            TextMemory.MetaTag metaTag = TextMemory.Parse(info);
            switch (metaTag.group) {
                case '?':
                    ThoughtPopup.Instance.Open(metaTag.key);
                    break;
                case '~':
                    if (metaTag.key == "village") {
                        GetTree().ChangeScene("res://Scenes/VillageScene.tscn");
                    } else if (metaTag.key == "combat") {
                        GetTree().ChangeScene("res://Scenes/CombatScene.tscn");
                    } else {
                        GD.Print("Unknown scene");
                    }
                    break;
            }
        }
    }
}
