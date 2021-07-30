// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Windows.Forms;
    using GoToBible.Windows.Properties;
    using Microsoft.Web.WebView2.Core;

    /// <summary>
    /// The GoToBible program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Gets the open forms.
        /// </summary>
        /// <value>
        /// The forms.
        /// </value>
        public static List<FormMain> Forms { get; } = new List<FormMain>();

        /// <summary>
        /// Gets the program title.
        /// </summary>
        /// <value>
        /// The program title.
        /// </value>
        public static string Title => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? @"GoTo.Bible";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [SupportedOSPlatform("windows")]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FormMain formMain = new FormMain(true);
            Forms.Add(formMain);
            formMain.Show();
            try
            {
                Application.Run();
            }
            catch (TargetInvocationException ex)
            {
                if (ex?.InnerException is WebView2RuntimeNotFoundException
                    && MessageBox.Show(string.Format(Resources.WebViewNotFound), $@"Cannot Start {Title}", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo("https://go.microsoft.com/fwlink/p/?LinkId=2124703")
                    {
                        UseShellExecute = true,
                        Verb = "open",
                    });
                }
            }
        }
    }
}
