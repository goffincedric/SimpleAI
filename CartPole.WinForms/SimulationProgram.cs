using System;
using System.Windows.Forms;
using CartPoleWinForms.Views;

namespace CartPoleWinForms;

static class SimulationProgram
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void RunSimulation()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SimulationForm());
    }
}
