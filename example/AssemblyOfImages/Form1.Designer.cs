namespace AssemblyOfImages
{
	partial class Form1 : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( Form1 ) );
			this.imageList1 = new System.Windows.Forms.ImageList( this.components );
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject( "imageList1.ImageStream" )));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName( 0, "interlock.bmp" );
			this.imageList1.Images.SetKeyName( 1, "diamond.bmp" );
			this.imageList1.Images.SetKeyName( 2, "Holy Shit.bmp" );
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point( 89, 89 );
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size( 209, 13 );
			this.label1.TabIndex = 0;
			this.label1.Text = "THIS IS A TEST....THIS IS ONLY A TEST";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 683, 464 );
			this.Controls.Add( this.label1 );
			this.Icon = ((System.Drawing.Icon)(resources.GetObject( "$this.Icon" )));
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.Label label1;
	}
}