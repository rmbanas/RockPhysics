using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RockPhysics
{
    class Field : FlowLayoutPanel
    {
        public Label label;
        public TextBox text_box;

        public Field(string name, string value)
            : base()
        {
            AutoSize = true;

            label = new Label();
            label.Name = "lbl" + name;
            label.Text = name;
            label.AutoSize = true;
            label.Anchor = AnchorStyles.Left;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            Controls.Add(label);

            text_box = new TextBox();
            text_box.Name = "txt" + name;
            text_box.Text = value;

            Controls.Add(text_box);
        }
    }
}
