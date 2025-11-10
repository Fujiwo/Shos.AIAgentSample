using System.ComponentModel;

namespace FCAICad
{
    partial class MainForm
    {
        /// <summary>Required designer variable.</summary>
        IContainer components = null;

        /// <summary>Clean up any resources being used.</summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            view = new View();
            promptTextBox = new TextBox();
            promptButton = new Button();
            responseTextBox = new TextBox();
            SuspendLayout();
            // 
            // view
            // 
            view.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            view.BackColor = Color.White;
            view.Location = new Point(-5, -1);
            view.Name = "view";
            view.Size = new Size(1173, 859);
            view.TabIndex = 0;
            // 
            // promptTextBox
            // 
            promptTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            promptTextBox.BackColor = Color.FromArgb(255, 255, 192);
            promptTextBox.ForeColor = Color.FromArgb(64, 64, 0);
            promptTextBox.Location = new Point(-4, 864);
            promptTextBox.Multiline = true;
            promptTextBox.Name = "promptTextBox";
            promptTextBox.Size = new Size(1059, 165);
            promptTextBox.TabIndex = 1;
            // 
            // promptButton
            // 
            promptButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            promptButton.Location = new Point(1061, 864);
            promptButton.Name = "promptButton";
            promptButton.Size = new Size(102, 165);
            promptButton.TabIndex = 2;
            promptButton.Text = "▶";
            promptButton.UseVisualStyleBackColor = true;
            promptButton.Click += OnPromptButtonClick;
            // 
            // responseTextBox
            // 
            responseTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            responseTextBox.Location = new Point(6, 1035);
            responseTextBox.Multiline = true;
            responseTextBox.Name = "responseTextBox";
            responseTextBox.ReadOnly = true;
            responseTextBox.Size = new Size(1157, 289);
            responseTextBox.TabIndex = 3;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(20F, 48F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1164, 1326);
            Controls.Add(responseTextBox);
            Controls.Add(promptButton);
            Controls.Add(promptTextBox);
            Controls.Add(view);
            Name = "MainForm";
            Text = "FCAICad";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        View view;
        TextBox promptTextBox;
        Button promptButton;
        TextBox responseTextBox;
    }
}
