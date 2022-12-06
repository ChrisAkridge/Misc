using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Providers;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public sealed class StaticURLImagePresenter : IPresenter
    {
        private readonly StaticURLImageProvider provider = new StaticURLImageProvider();
        private readonly Settings settings;
        private readonly Label label;
        private readonly PictureBox pictureBox;

        public StaticURLImagePresenter(Panel panel, Settings settings, int startingY, out int endingY)
        {
            this.settings = settings;
            
            label = new Label
            {
                Location = new Point(5, startingY),
                AutoSize = true,
                MaximumSize = new Size(panel.Width - 10, 0),
                Text = "Loading image..."
            };

            startingY += TextRenderer.MeasureText(label.Text, label.Font).Height + 5;

            pictureBox = new PictureBox
            {
                Location = new Point(5, startingY),
                Size = new Size(panel.Width - 10, 406),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            startingY += pictureBox.Height + 5;

            var separatorLabel = new Label
            {
                AutoSize = false,
                Location = new Point(5, startingY),
                Size = new Size(panel.Width - 10, 2),
                BackColor = SystemColors.ControlDark,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            
            panel.Controls.Add(label);
            panel.Controls.Add(pictureBox);
            panel.Controls.Add(separatorLabel);

            endingY = startingY + separatorLabel.Height + 5;
            
            Render(0);
        }

        public void Render(int timerTicks)
        {
            if (timerTicks % settings.StaticURLImagePresenterUpdateTime == 0)
            {
                var labelAndPath = provider.GetDisplayObject();
                label.Text = labelAndPath.Key;
                
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (labelAndPath.Value != null)
                {
                    try
                    {
                        using var stream = File.OpenRead(labelAndPath.Value);
                        pictureBox.Image = Image.FromStream(stream);
                    }
                    catch (Exception ex)
                    {
                        pictureBox.Image = null;
                        label.Text = ex.Message;
                    }
                }
                else
                {
                    pictureBox.Image = null;
                }
            }
        }
    }
}
