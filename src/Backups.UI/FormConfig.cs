using System;
using System.Windows.Forms;
namespace Backups.UI;
public partial class FormConfig : Form
{
    private Button btnEnviar;
    public FormConfig() { InitializeComponent(); }
    private void InitializeComponent()
    {
        this.btnEnviar = new Button();
        this.SuspendLayout();
        this.btnEnviar.Location = new System.Drawing.Point(100, 100);
        this.btnEnviar.Size = new System.Drawing.Size(120, 40);
        this.btnEnviar.Text = "Enviar Respaldo";
        this.btnEnviar.Click += new EventHandler(this.BtnEnviar_Click);
        this.ClientSize = new System.Drawing.Size(350, 250);
        this.Controls.Add(this.btnEnviar);
        this.Text = "ConfiguraciÃ³n de Backups - Grupo 4";
        this.ResumeLayout(false);
    }
    private void BtnEnviar_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("Disparando caso de uso de respaldo manual...");
    }
}
