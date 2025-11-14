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
            clearButton = new Button();
            SuspendLayout();
            // 
            // view
            // 
            view.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            view.BackColor = Color.White;
            view.Location = new Point(0, 0);
            view.Name = "view";
            view.Size = new Size(2363, 1134);
            view.TabIndex = 0;
            // 
            // promptTextBox
            // 
            promptTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            promptTextBox.BackColor = Color.FromArgb(255, 255, 192);
            promptTextBox.ForeColor = Color.FromArgb(64, 64, 0);
            promptTextBox.Location = new Point(0, 1140);
            promptTextBox.Multiline = true;
            promptTextBox.Name = "promptTextBox";
            promptTextBox.Size = new Size(2156, 150);
            promptTextBox.TabIndex = 1;
            // 
            // promptButton
            // 
            promptButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            promptButton.Font = new Font("Yu Gothic UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 128);
            promptButton.Location = new Point(2158, 1140);
            promptButton.Name = "promptButton";
            promptButton.Size = new Size(102, 150);
            promptButton.TabIndex = 2;
            promptButton.Text = "🤖";
            promptButton.UseVisualStyleBackColor = true;
            promptButton.Click += OnPromptButtonClick;
            // 
            // responseTextBox
            // 
            responseTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            responseTextBox.BackColor = Color.FromArgb(192, 255, 255);
            responseTextBox.ForeColor = Color.FromArgb(0, 64, 64);
            responseTextBox.Location = new Point(0, 1296);
            responseTextBox.Multiline = true;
            responseTextBox.Name = "responseTextBox";
            responseTextBox.ReadOnly = true;
            responseTextBox.Size = new Size(2363, 200);
            responseTextBox.TabIndex = 4;
            // 
            // clearButton
            // 
            clearButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            clearButton.Font = new Font("Yu Gothic UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 128);
            clearButton.Location = new Point(2261, 1140);
            clearButton.Name = "clearButton";
            clearButton.Size = new Size(102, 150);
            clearButton.TabIndex = 3;
            clearButton.Text = "🆑";
            clearButton.UseVisualStyleBackColor = true;
            clearButton.Click += OnClearButtonClick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(20F, 48F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2364, 1497);
            Controls.Add(view);
            Controls.Add(promptTextBox);
            Controls.Add(promptButton);
            Controls.Add(clearButton);
            Controls.Add(responseTextBox);
            Name = "MainForm";
            Text = "FCAICad";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        View view;
        TextBox promptTextBox;
        Button promptButton;
        Button clearButton;
        TextBox responseTextBox;
    }
}
