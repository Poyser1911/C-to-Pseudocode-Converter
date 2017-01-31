using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.IO;
using MahApps.Metro.Controls.Dialogs;
using ColorCode;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using MahApps.Metro.Controls;
using System.Windows.Media.Animation;

namespace C_to_Pseudocode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        FilterTrakr options = new FilterTrakr();
        bool WindowLoaded = false;
        public MainWindow()
        {
            InitializeComponent();
            code1.AcceptsTab = true;
            versionbox.SelectedIndex = 1;
            LoadFilter();
        }
        public void LoadFilter()
        {
            specialequal.IsChecked = options.filter.UseArrowEqual;
            nofuncpro.IsChecked = options.filter.RemoveFuncPrototype;
            autointenting.IsChecked = options.filter.EnableAutoIndent;
            liveupdate.IsChecked = options.filter.EnableLiveUpdate;
            nocomments.IsChecked = options.filter.RemoveComments;
            nocolours.IsChecked = options.filter.RemoveColours;
            noincludes.IsChecked = options.filter.RemoveIncludes;
        }

        private void code1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (options.filter.EnableLiveUpdate)
                Convert(GetText(code1));
        }

        public string GetText(RichTextBox box)
        {
            return new TextRange(box.Document.ContentStart, box.Document.ContentEnd).Text;
        }
        public void Convert(string code)
        {
            LineParser parser = new LineParser();
            parser.ProgressChanged += parser_ProgressChanged;
            try
            {
                SetText(code2, parser.Parse(code, typebox.SelectionBoxItem.ToString(), options.filter));
            }
            catch (Exception) { }
        }

        void parser_ProgressChanged(ProgressChangedEventArgs e)
        {
            //Dispatcher.Invoke(new Action(() => { progress.Value = e.percent+6; progress2.Content = (e.percent+6)+"%"; }));
        }
        public void SetText(RichTextBox box, string code)
        {
            box.Document.Blocks.Clear();
            box.Document.Blocks.Add(new Paragraph(new Run(code)));
        }

        private void convert_Click(object sender, RoutedEventArgs e)
        {
            Convert(GetText(code1));
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
                File.WriteAllText("run.c", GetText(code1));
                File.WriteAllText("run.bat", "call \"C:\\Program Files (x86)\\Microsoft Visual Studio 11.0\\VC\\vcvarsall.bat\" x86_amd64\ncall \"c:\\Program Files (x86)\\Microsoft visual Studio 11.0\\VC\\bin\\amd64\\cl.exe\" run.c");
                ProcessStartInfo p = new ProcessStartInfo("run.bat");
                p.CreateNoWindow = true;
                Process.Start(p).WaitForExit();
                Process.Start("run.exe");
            }
            catch (Exception e) { MessageBox.Show("Error", e.Message); }
        }

        private void run_Click(object sender, RoutedEventArgs e)
        {
            Run();
        }

        private void code1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
                e.Handled = true;
        }

        private void MetroWindow_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (File.Exists("run.c"))
            {
                SetText(code1, File.ReadAllText("run.c"));
            }
            WindowLoaded = true;
        }

        private void loadfile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog file = new System.Windows.Forms.OpenFileDialog();
            file.Filter = "C Source File(*.c)|*.c|All Files(*.*)|*.*";
            file.Title = "Open File with C code";
            if (file.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            SetText(code1, File.ReadAllText(file.FileName));

        }

        private async void about_Click(object sender, RoutedEventArgs e)
        {
            await this.ShowMessageAsync("Creator", "Contact: poyser1911@gmail.com");
        }

        private void filterchanged(object sender, RoutedEventArgs e)
        {
            if (!WindowLoaded)
                return;
            string lable = (sender as CheckBox).Content.ToString();
            bool value = (bool)(sender as CheckBox).IsChecked;
            //MessageBox.Show(changed);

            switch (lable)
            {
                case "Use ← For (=) Symbol": options.filter.UseArrowEqual = value; break;
                case "Remove Function declarations": options.filter.RemoveFuncPrototype = value; break;
                case "Auto Indenting": options.filter.EnableAutoIndent = value; break;
                case "Live Update": options.filter.EnableLiveUpdate = value; break;
                case "Remove Comments": options.filter.RemoveComments = value; break;
                case "Remove Colours": options.filter.RemoveColours = value; break;
                case "Remove Includes": options.filter.RemoveIncludes = value; break;
                default:
                    break;
            }
            options.Save();
            Convert(GetText(code1));
        }

    }
}
