namespace EvoANTFrontend
{
	partial class NeuronViewerForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.neuronViewer1 = new NeuronViewer(null);
			this.SuspendLayout();
			// 
			// neuronViewer1
			// 
			this.neuronViewer1.Ant = null;
			this.neuronViewer1.Location = new System.Drawing.Point(13, 13);
			this.neuronViewer1.Name = "neuronViewer1";
			this.neuronViewer1.Size = new System.Drawing.Size(376, 313);
			this.neuronViewer1.TabIndex = 0;
			// 
			// NeuronViewerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(401, 338);
			this.Controls.Add(this.neuronViewer1);
			this.Name = "NeuronViewerForm";
			this.Text = "NeuronViewerForm";
			this.ResumeLayout(false);

		}

		#endregion

		private NeuronViewer neuronViewer1;
	}
}