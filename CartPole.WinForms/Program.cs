using System;
using System.Windows.Forms;
using CartPoleWinForms.Views;

namespace CartPoleWinForms;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main()
    {
        Application.Run(new RenderForm());
    }
}
