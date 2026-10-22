using System.Drawing;
using System.Windows.Forms;
using GestionParqueo.UI.Controles;
using LibreriaClases.Logica;

namespace GestionParqueo.UI;

public class FrmPrincipal : Form
{
    private readonly MenuStrip menu = new MenuStrip();
    private readonly Panel contenedor = new Panel { Dock = DockStyle.Fill };

    // Las pantallas restantes se habilitarán cuando existan sus controles.
    private readonly ToolStripMenuItem clientes = new ToolStripMenuItem("Clientes") { Enabled = false };
    private readonly ToolStripMenuItem vehiculos = new ToolStripMenuItem("Vehículos") { Enabled = false };
    private readonly ToolStripMenuItem espacios = new ToolStripMenuItem("Espacios") { Enabled = false };
    private readonly ToolStripMenuItem tarifas = new ToolStripMenuItem("Tarifas") { Enabled = false };
    private readonly ToolStripMenuItem entradasSalidas = new ToolStripMenuItem("Entradas y salidas") { Enabled = false };
    private readonly ToolStripMenuItem pagos = new ToolStripMenuItem("Pagos") { Enabled = false };
    private readonly ToolStripMenuItem reportes = new ToolStripMenuItem("Reportes") { Enabled = false };

    public FrmPrincipal()
    {
        Text = "Gestión de Parqueo";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 800;
        Height = 520;
        MinimumSize = new Size(600, 360);

        ToolStripMenuItem informacionBase = new ToolStripMenuItem("Información Base");
        informacionBase.DropDownItems.AddRange(new ToolStripItem[] { clientes, vehiculos, espacios, tarifas });

        ToolStripMenuItem movimientos = new ToolStripMenuItem("Movimientos");
        movimientos.DropDownItems.AddRange(new ToolStripItem[] { entradasSalidas, pagos });

        ToolStripMenuItem consultasReportes = new ToolStripMenuItem("Consultas y Reportes");
        consultasReportes.DropDownItems.Add(reportes);

        ToolStripMenuItem sesion = new ToolStripMenuItem("Sesión");
        ToolStripMenuItem cerrarSesion = new ToolStripMenuItem("Cerrar sesión");
        cerrarSesion.Click += (_, _) => AbrirLogin();
        sesion.DropDownItems.Add(cerrarSesion);

        menu.Items.AddRange(new ToolStripItem[] { informacionBase, movimientos, consultasReportes, sesion });
        MainMenuStrip = menu;
        Controls.Add(contenedor);
        Controls.Add(menu);
        AbrirLogin();
    }

    public void CargarControl(UserControl uc)
    {
        if (uc == null) throw new ArgumentNullException(nameof(uc));

        // Desechar la pantalla anterior evita controles y eventos olvidados.
        while (contenedor.Controls.Count > 0)
        {
            Control anterior = contenedor.Controls[0];
            contenedor.Controls.RemoveAt(0);
            anterior.Dispose();
        }

        uc.Dock = DockStyle.Fill;
        contenedor.Controls.Add(uc);
    }

    private void AbrirLogin()
    {
        Sesion.Cerrar();
        menu.Visible = false;
        UcLogin login = new UcLogin();
        // Ejecutar después del clic permite retirar el control de login sin interrumpirlo.
        login.AccesoConcedido += (_, _) => BeginInvoke(new Action(MostrarSesion));
        CargarControl(login);
    }

    private void MostrarSesion()
    {
        if (Sesion.EmpleadoActual == null) return;

        bool administrador = Sesion.EsAdministrador;
        clientes.Visible = true;
        vehiculos.Visible = true;
        espacios.Visible = administrador;
        tarifas.Visible = administrador;
        entradasSalidas.Visible = true;
        pagos.Visible = true;
        reportes.Visible = true;

        UserControl inicio = new UserControl();
        Label bienvenida = new Label
        {
            Text = "Bienvenido, " + Sesion.EmpleadoActual.NombreCompleto + ". Seleccione una opción del menú.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        inicio.Controls.Add(bienvenida);
        CargarControl(inicio);
        menu.Visible = true;
    }
}
