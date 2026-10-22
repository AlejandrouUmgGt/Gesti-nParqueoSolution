using System.Drawing;
using System.Windows.Forms;
using LibreriaClases.Logica;
using Microsoft.Data.SqlClient;

namespace GestionParqueo.UI.Controles;

public class UcLogin : UserControl
{
    private readonly TextBox txtUsuario = new TextBox { Location = new Point(135, 77), Width = 200, MaxLength = 50 };
    private readonly TextBox txtClave = new TextBox
    {
        Location = new Point(135, 117), Width = 200, MaxLength = 200,
        UseSystemPasswordChar = true
    };
    private readonly Button btnIngresar = new Button { Text = "Ingresar", Location = new Point(135, 160), Width = 100 };

    public event EventHandler? AccesoConcedido;

    public UcLogin()
    {
        Label titulo = new Label { Text = "Iniciar sesión", Location = new Point(30, 25), AutoSize = true };
        Label lblUsuario = new Label { Text = "Usuario:", Location = new Point(30, 80), AutoSize = true };
        Label lblClave = new Label { Text = "Contraseña:", Location = new Point(30, 120), AutoSize = true };

        Controls.AddRange(new Control[] { titulo, lblUsuario, txtUsuario, lblClave, txtClave, btnIngresar });
        btnIngresar.Click += (_, _) => Ingresar();
        txtClave.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            Ingresar();
        };
    }

    private void Ingresar()
    {
        if (string.IsNullOrWhiteSpace(txtUsuario.Text) || txtClave.Text.Length == 0)
        {
            MessageBox.Show(this, "Escriba usuario y contraseña.", "Inicio de sesión",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        btnIngresar.Enabled = false;
        try
        {
            if (Sesion.Iniciar(txtUsuario.Text.Trim(), txtClave.Text))
            {
                txtClave.Clear();
                AccesoConcedido?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                txtClave.Clear();
                MessageBox.Show(this, "Usuario o contraseña incorrectos, o empleado inactivo.",
                    "Inicio de sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (SqlException error)
        {
            txtClave.Clear();
            MessageBox.Show(this, "No se pudo consultar la base de datos: " + error.Message,
                "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnIngresar.Enabled = true;
        }
    }
}
