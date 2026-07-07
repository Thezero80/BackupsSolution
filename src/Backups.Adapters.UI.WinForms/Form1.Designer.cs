namespace Backups.Adapters.UI.WinForms;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        btnSeleccionar = new Button();
        btnEnviar = new Button();
        label1 = new Label();
        lstCopias = new ListBox();
        lstRegistro = new ListBox();
        menuStrip1 = new MenuStrip();
        notifyIcon1 = new NotifyIcon(components);
        contextMenuStrip1 = new ContextMenuStrip(components);
        statusStrip1 = new StatusStrip();
        toolStripMenuItem1 = new ToolStripMenuItem();
        toolStripMenuItem2 = new ToolStripMenuItem();
        toolStripMenuItem3 = new ToolStripMenuItem();
        menuStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // btnSeleccionar
        // 
        btnSeleccionar.Location = new Point(44, 60);
        btnSeleccionar.Name = "btnSeleccionar";
        btnSeleccionar.Size = new Size(103, 40);
        btnSeleccionar.TabIndex = 0;
        btnSeleccionar.Text = "Seleccionar archivos";
        btnSeleccionar.UseVisualStyleBackColor = true;
        // 
        // btnEnviar
        // 
        btnEnviar.Location = new Point(44, 115);
        btnEnviar.Name = "btnEnviar";
        btnEnviar.Size = new Size(103, 38);
        btnEnviar.TabIndex = 1;
        btnEnviar.Text = "Enviar Copia";
        btnEnviar.UseVisualStyleBackColor = true;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(44, 179);
        label1.Name = "label1";
        label1.Size = new Size(213, 15);
        label1.TabIndex = 2;
        label1.Text = "ARCHIVO   NRO FECHA HORA ESTADO";
        // 
        // lstCopias
        // 
        lstCopias.FormattingEnabled = true;
        lstCopias.Location = new Point(44, 211);
        lstCopias.Name = "lstCopias";
        lstCopias.Size = new Size(459, 214);
        lstCopias.TabIndex = 3;
        // 
        // lstRegistro
        // 
        lstRegistro.FormattingEnabled = true;
        lstRegistro.Location = new Point(44, 442);
        lstRegistro.Name = "lstRegistro";
        lstRegistro.Size = new Size(459, 94);
        lstRegistro.TabIndex = 4;
        // 
        // menuStrip1
        // 
        menuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem2, toolStripMenuItem3 });
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(570, 24);
        menuStrip1.TabIndex = 5;
        menuStrip1.Text = "menuStrip1";
        menuStrip1.ItemClicked += menuStrip1_ItemClicked;
        // 
        // notifyIcon1
        // 
        notifyIcon1.Text = "notifyIcon1";
        notifyIcon1.Visible = true;
        // 
        // contextMenuStrip1
        // 
        contextMenuStrip1.Name = "contextMenuStrip1";
        contextMenuStrip1.Size = new Size(61, 4);
        // 
        // statusStrip1
        // 
        statusStrip1.Location = new Point(0, 553);
        statusStrip1.Name = "statusStrip1";
        statusStrip1.Size = new Size(570, 22);
        statusStrip1.TabIndex = 7;
        statusStrip1.Text = "statusStrip1";
        // 
        // toolStripMenuItem1
        // 
        toolStripMenuItem1.Name = "toolStripMenuItem1";
        toolStripMenuItem1.Size = new Size(60, 20);
        toolStripMenuItem1.Text = "Archivo";
        // 
        // toolStripMenuItem2
        // 
        toolStripMenuItem2.Name = "toolStripMenuItem2";
        toolStripMenuItem2.Size = new Size(49, 20);
        toolStripMenuItem2.Text = "Editar";
        // 
        // toolStripMenuItem3
        // 
        toolStripMenuItem3.Name = "toolStripMenuItem3";
        toolStripMenuItem3.Size = new Size(24, 20);
        toolStripMenuItem3.Text = "?";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(570, 575);
        Controls.Add(statusStrip1);
        Controls.Add(lstRegistro);
        Controls.Add(lstCopias);
        Controls.Add(label1);
        Controls.Add(btnEnviar);
        Controls.Add(btnSeleccionar);
        Controls.Add(menuStrip1);
        MainMenuStrip = menuStrip1;
        Name = "Form1";
        Text = "Form1";
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button btnSeleccionar;
    private Button btnEnviar;
    private Label label1;
    private ListBox lstCopias;
    private ListBox lstRegistro;
    private MenuStrip menuStrip1;
    private NotifyIcon notifyIcon1;
    private ContextMenuStrip contextMenuStrip1;
    private StatusStrip statusStrip1;
    private ToolStripMenuItem toolStripMenuItem1;
    private ToolStripMenuItem toolStripMenuItem2;
    private ToolStripMenuItem toolStripMenuItem3;
}
