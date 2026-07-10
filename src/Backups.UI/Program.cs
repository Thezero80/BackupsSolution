using System;
using System.Windows.Forms;
namespace Backups.UI;
static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormConfig());
    }
}
