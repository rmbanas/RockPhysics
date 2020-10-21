using System.Windows.Forms;

namespace RockPhysics
{
    /// <summary>
    /// This is a class to encapsulate both a label and text box together since VisualStudio doesn't allow this functionality
    /// Viz. idiots
    /// </summary>
    class Combo : FlowLayoutPanel
    {
        // Label and text box
        public Label label;
        public ComboBox combo_box;

        /// <summary>
        /// Inherited constructor from FlowLayoutPanel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public Combo(string name, string[] value)
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

            combo_box = new ComboBox();
            combo_box.Name = "cbo" + name;
            foreach(string s in value)
            {
                combo_box.Items.Add(s);
            }

            combo_box.SelectedIndex = 0;
            Controls.Add(combo_box);
        }
    }
}