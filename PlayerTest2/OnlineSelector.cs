using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace PlayerTest2
{
    class OnlineSelector : ComboBox
    {
        public OnlineSelector()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        // Draws the items into the ColorSelector object
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();
                e.DrawFocusRectangle();
                FollowingStreams stream = Form1.GetStreamByName(Items[e.Index].ToString());
                DropDownItem item = null;
                if(stream.Online)
                    item = new DropDownItem(Items[e.Index].ToString(), Color.Green);
                else
                    item = new DropDownItem(Items[e.Index].ToString(), Color.Red);
                // Draw the colored 16 x 16 square
                e.Graphics.DrawImage(item.Image, e.Bounds.Left, e.Bounds.Top);
                // Draw the value (in this case, the color name)
                e.Graphics.DrawString(item.Value, e.Font, new
                        SolidBrush(e.ForeColor), e.Bounds.Left + item.Image.Width, e.Bounds.Top + 2);

                base.OnDrawItem(e);
            }
        }
    }


    public class DropDownItem
    {
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
        private string value;

        public Image Image
        {
            get { return img; }
            set { img = value; }
        }
        private Image img;

        public DropDownItem()
            : this("", Color.Empty)
        { }

        public DropDownItem(string val, Color color)
        {
            value = val;
            this.img = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(img);
            Brush b = new SolidBrush(color);
            g.DrawRectangle(Pens.White, 0, 0, img.Width, img.Height);
            g.FillRectangle(b, 1, 1, img.Width - 1, img.Height - 1);
        }

        public override string ToString()
        {
            return value;
        }
    }
}
