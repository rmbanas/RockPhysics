using System.Windows.Forms;

namespace RockPhysics
{
    /// <summary>
    /// This is a class to encapsulate both a label and text box together since VisualStudio doesn't allow this functionality
    /// Viz. idiots
    /// </summary>
    class Field : FlowLayoutPanel
    {
        // Label and text box
        public Label label;
        public TextBox text_box;

        /// <summary>
        /// Inherited constructor from FlowLayoutPanel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
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
