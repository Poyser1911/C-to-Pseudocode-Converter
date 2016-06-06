using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Diagnostics;
using System.IO;
using MahApps.Metro.Controls.Dialogs;
namespace C_to_Pseudocode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            code1.AcceptsTab = true;
            versionbox.SelectedIndex = 1;
        }

        private void code1_TextChanged(object sender, TextChangedEventArgs e)
        {

            Convert(GetCode(code1));
        }

        public string GetCode(RichTextBox box)
        {
            return new TextRange(box.Document.ContentStart, box.Document.ContentEnd).Text;
        }
        public void Convert(string code)
        {
            //string type = typebox.SelectedIndex == 0 ? "Function":"Struct";
            try
            {
                SetPreview(code2, LineParser.Parse(code, typebox.SelectionBoxItem.ToString()));
            }
            catch (Exception e) { try { /*this.ShowMessageAsync("Error", e.Message);*/ } catch (Exception) { } }
        }
        public void SetPreview(RichTextBox box,string code)
        {
            box.Document.Blocks.Clear();
            box.Document.Blocks.Add(new Paragraph(new Run(code)));
        }

        private void convert_Click(object sender, RoutedEventArgs e)
        {
            Convert(GetCode(code1));
        }

        private void typebox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (typebox.SelectedIndex == 0)
                    box1heading.Content = "Insert C Code Function";
                else
                    box1heading.Content = "Insert C Code Struct";
            }
            catch (Exception) { }
        }
        public async void Run()
        {
            try
            {

                string path1 = @"c:\Program Files (x86)\Microsoft visual Studio 11.0\VC\bin\amd64\cl.exe";
                string path2 = @"c:\Program Files\Microsoft Visual Studio 11.0\VC\bin\x86_amd64\cl.exe";
                string filename;

                if (File.Exists(path1))
                    filename = path1;
                else if (File.Exists(path2))
                    filename = path2;
                else
                {
                    await this.ShowMessageAsync("Error", "Visual Studio Dev Compiler Not found");
                    return;
                }
                File.WriteAllText("run.c", GetCode(code1));
                File.WriteAllText("run.bat", "call \"C:\\Program Files (x86)\\Microsoft Visual Studio 11.0\\VC\\vcvarsall.bat\" x86_amd64\ncall \"c:\\Program Files (x86)\\Microsoft visual Studio 11.0\\VC\\bin\\amd64\\cl.exe\" run.c");
                ProcessStartInfo p = new ProcessStartInfo("run.bat");
                p.CreateNoWindow = true;
                Process.Start(p).WaitForExit();
                Process.Start("run.exe");
            }
            catch (Exception e) { this.ShowMessageAsync("Error",e.Message ); }
        }

        private void run_Click(object sender, RoutedEventArgs e)
        {
            Run();
        }

        private void code1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)                e.Handled = true;
        }
        public async void Welcome()
        {
            this.ShowMessageAsync("Welcome "+System.Environment.UserName,"This tool is still in development.");
        }

        private void MetroWindow_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (File.Exists("run.c"))
                SetPreview(code1,File.ReadAllText("run.c"));
            //Welcome();
        }

        private void loadfile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog file = new System.Windows.Forms.OpenFileDialog();
            file.Filter = "C Source File(*.c)|*.c|All Files(*.*)|*.*";
            file.Title = "Open File with C code";
            if (file.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            SetPreview(code1,File.ReadAllText(file.FileName));

        }

        private async void about_Click(object sender, RoutedEventArgs e)
        {
            await this.ShowMessageAsync("Creator", "Contact: poyser1911@gmail.com");
        }

    }
}
