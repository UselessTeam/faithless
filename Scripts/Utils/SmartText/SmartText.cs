using System;
using Godot;

namespace Utils {
    public class SmartText : RichTextLabel {
        public override void _Ready() {
            Connect("meta_clicked", this, nameof(MetaClicked));
        }

        private void MetaClicked(string info) {
            TextMemory.MetaTag metaTag = TextMemory.Parse(info);
            // Do something with the MetaTag
        }
    }
}
