using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Backups.Core.Application.UseCases;
using Backups.Core.Domain.Entities;
using Backups.Core.Domain.Services;
using Backups.Core.Ports.In;
using Backups.Core.Ports.Out;
using Backups.Infrastructure.Compression;
using Backups.Infrastructure.Network;

namespace Backups.UI;

public partial class FormConfig : Form
{
    private readonly IEjecutarRespaldoUseCase _useCase;
    private readonly IHistorialRespaldoService _historialService;

    private TextBox txtRutaOrigen = null!;
    private TextBox txtNombreCopia = null!;
    private ComboBox cmbAlgoritmo = null!;
    private NumericUpDown numLimiteMb = null!;
    private ComboBox cmbDestino = null!;
    private Button btnSeleccionar = null!;
    private Button btnEnviar = null!;
    private ListBox lstHistorial = null!;
    private Label lblEstado = null!;

    public FormConfig()
    {
        InitializeComponent();

        var gestorLog = new GestorLog();
        _historialService = gestorLog;
        _useCase = new EjecutarRespaldoUseCase(
            new VerificadorCambios(),
            gestorLog,
            new CompressionAdapter(),
            new NetworkTransmissionAdapter());

        CargarHistorial();
    }

    private void InitializeComponent()
    {
        var lblArchivo = new Label { Text = "Archivo a respaldar:", Location = new Point(20, 20), AutoSize = true };
        txtRutaOrigen = new TextBox { Location = new Point(20, 40), Size = new Size(330, 23), ReadOnly = true };
        btnSeleccionar = new Button { Text = "Seleccionar...", Location = new Point(360, 39), Size = new Size(100, 25) };
        btnSeleccionar.Click += BtnSeleccionar_Click;

        var lblNombre = new Label { Text = "Nombre de la copia:", Location = new Point(20, 75), AutoSize = true };
        txtNombreCopia = new TextBox { Location = new Point(20, 95), Size = new Size(440, 23) };

        var lblAlgoritmo = new Label { Text = "Algoritmo:", Location = new Point(20, 130), AutoSize = true };
        cmbAlgoritmo = new ComboBox { Location = new Point(20, 150), Size = new Size(140, 23), DropDownStyle = ComboBoxStyle.DropDownList };
        cmbAlgoritmo.Items.AddRange(new object[] { "ZIP", "LZMA", "RAR" });
        cmbAlgoritmo.SelectedIndex = 0;

        var lblLimite = new Label { Text = "Límite volumen (MB):", Location = new Point(180, 130), AutoSize = true };
        numLimiteMb = new NumericUpDown { Location = new Point(180, 150), Size = new Size(100, 23), Minimum = 1, Maximum = 100000, Value = 100 };

        var lblDestino = new Label { Text = "Destino (config):", Location = new Point(300, 130), AutoSize = true };
        cmbDestino = new ComboBox { Location = new Point(300, 150), Size = new Size(160, 23), DropDownStyle = ComboBoxStyle.DropDown };
        cmbDestino.Items.AddRange(new object[] { "ftp_servidor_1", "sftp_servidor_1" });
        cmbDestino.SelectedIndex = 0;

        btnEnviar = new Button { Text = "Enviar Respaldo", Location = new Point(20, 190), Size = new Size(150, 40) };
        btnEnviar.Click += BtnEnviar_Click;

        lblEstado = new Label { Text = "Sin respaldos registrados", Location = new Point(190, 200), AutoSize = true };

        var lblHistorial = new Label { Text = "Historial:", Location = new Point(20, 245), AutoSize = true };
        lstHistorial = new ListBox { Location = new Point(20, 265), Size = new Size(440, 180), HorizontalScrollbar = true };

        SuspendLayout();
        ClientSize = new Size(480, 465);
        Controls.Add(lblArchivo);
        Controls.Add(txtRutaOrigen);
        Controls.Add(btnSeleccionar);
        Controls.Add(lblNombre);
        Controls.Add(txtNombreCopia);
        Controls.Add(lblAlgoritmo);
        Controls.Add(cmbAlgoritmo);
        Controls.Add(lblLimite);
        Controls.Add(numLimiteMb);
        Controls.Add(lblDestino);
        Controls.Add(cmbDestino);
        Controls.Add(btnEnviar);
        Controls.Add(lblEstado);
        Controls.Add(lblHistorial);
        Controls.Add(lstHistorial);
        Text = "WinBackup - Configuración de Respaldos";
        ResumeLayout(false);
        PerformLayout();
    }

    private void BtnSeleccionar_Click(object? sender, EventArgs e)
    {
        using var dialogo = new OpenFileDialog { Title = "Seleccionar archivo a respaldar" };
        if (dialogo.ShowDialog() != DialogResult.OK)
            return;

        txtRutaOrigen.Text = dialogo.FileName;
        if (string.IsNullOrWhiteSpace(txtNombreCopia.Text))
            txtNombreCopia.Text = Path.GetFileNameWithoutExtension(dialogo.FileName);
    }

    private void BtnEnviar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtRutaOrigen.Text) || !File.Exists(txtRutaOrigen.Text))
        {
            MessageBox.Show("Selecciona un archivo válido para respaldar.", "WinBackup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(cmbDestino.Text))
        {
            MessageBox.Show("Indica el destino de configuración.", "WinBackup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var solicitud = new SolicitudRespaldo
        {
            NombreCopia = string.IsNullOrWhiteSpace(txtNombreCopia.Text) ? Path.GetFileName(txtRutaOrigen.Text) : txtNombreCopia.Text,
            TipoDisparo = "MANUAL",
            RutaOrigen = txtRutaOrigen.Text,
            UltimoHashConocido = string.Empty,
            AlgoritmoCompresion = cmbAlgoritmo.SelectedItem?.ToString() ?? "ZIP",
            LimiteVolumenMb = (int)numLimiteMb.Value,
            IdDestinoConfig = cmbDestino.Text,
        };

        btnEnviar.Enabled = false;
        lblEstado.Text = "Procesando...";
        Cursor = Cursors.WaitCursor;

        try
        {
            _useCase.Ejecutar(solicitud);
        }
        finally
        {
            Cursor = Cursors.Default;
            btnEnviar.Enabled = true;
            CargarHistorial();
        }
    }

    private void CargarHistorial()
    {
        var historial = _historialService.ObtenerHistorial();

        lstHistorial.Items.Clear();
        foreach (var registro in historial.OrderByDescending(r => r.FechaRegistro))
        {
            lstHistorial.Items.Add($"{registro.FechaRegistro:yyyy-MM-dd HH:mm:ss}  [{registro.Estado}]  {registro.MensajeTexto}");
        }

        lblEstado.Text = historial.Count > 0
            ? $"Último estado: {historial[^1].Estado}"
            : "Sin respaldos registrados";
    }
}
