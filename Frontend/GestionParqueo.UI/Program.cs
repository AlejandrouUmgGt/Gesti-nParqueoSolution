using System.Windows.Forms;

namespace GestionParqueo.UI;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FrmPrincipal());
    }
}
