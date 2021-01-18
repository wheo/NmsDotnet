using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NmsDotnet.Utils
{
    public class InputBox
    {
        Window Box = new Window();//window for the inputbox
        FontFamily font = new FontFamily("NotoSans-Regular");//font for the whole inputbox
        int FontSize = 10;//fontsize for the input
        StackPanel sp1 = new StackPanel();// items container
        string title = "패널 입력";//title as heading
        string boxcontent;//title
        string defaulttext = "패널 이름을 입력 해주세요";//default textbox content
        string errormessage = "패널 이름을 입력 해주세요";//error messagebox content
        string errortitle = "에러";//error messagebox heading title
        string okbuttontext = "OK";//Ok button content
        Brush BoxBackgroundColor = Brushes.Silver;// Window Background
        Brush InputBackgroundColor = Brushes.White;// Textbox Background
        bool clicked = false;
        TextBox input = new TextBox();
        Button btnOk = new Button();
        Button btnCancel = new Button();
        bool inputreset = false;

        public InputBox(string content)
        {
            try
            {
                boxcontent = content;
            }
            catch { boxcontent = "Error!"; }
            windowdef();
        }

        public InputBox(string content, string Htitle, string DefaultText)
        {
            try
            {
                boxcontent = content;
            }
            catch { boxcontent = "Error!"; }
            try
            {
                title = Htitle;
            }
            catch
            {
                title = "Error!";
            }
            try
            {
                defaulttext = DefaultText;
            }
            catch
            {
                DefaultText = "Error!";
            }
            windowdef();
        }

        public InputBox(string content, string Htitle, string Font, int Fontsize)
        {
            try
            {
                boxcontent = content;
            }
            catch { boxcontent = "Error!"; }
            try
            {
                font = new FontFamily(Font);
            }
            catch { font = new FontFamily("NotoSans-Regular"); }
            try
            {
                title = Htitle;
            }
            catch
            {
                title = "Error!";
            }
            if (Fontsize >= 1)
                FontSize = Fontsize;
            windowdef();
        }

        private void windowdef()// window building - check only for window size
        {
            Box.Height = 100;// Box Height
            Box.Width = 350;// Box Width
            Box.Background = BoxBackgroundColor;
            Box.Title = title;
            sp1.Orientation = Orientation.Horizontal;
            Box.Content = sp1;
            Box.Closing += Box_Closing;
            /*
            TextBlock content = new TextBlock();
            content.TextWrapping = TextWrapping.Wrap;
            content.Background = null;
            content.HorizontalAlignment = HorizontalAlignment.Center;
            content.Text = boxcontent;
            content.FontFamily = font;
            content.FontSize = FontSize;
            content.Padding = new Thickness(5, 5, 5, 5);
            sp1.Children.Add(content);
            */

            input.Background = InputBackgroundColor;
            input.FontFamily = font;
            input.FontSize = FontSize;
            input.HorizontalAlignment = HorizontalAlignment.Center;
            input.Text = defaulttext;
            input.MinWidth = 200;
            input.Height = 50;
            input.MouseEnter += input_MouseDown;
            input.Margin = new Thickness(10);
            input.Padding = new Thickness(5, 5, 5, 5);
            sp1.Children.Add(input);
            btnOk.Width = 60;
            btnOk.Height = 50;
            btnOk.Click += ok_Click;
            btnOk.Content = okbuttontext;
            btnOk.HorizontalAlignment = HorizontalAlignment.Center;
            btnOk.Margin = new Thickness(10);
            btnOk.Padding = new Thickness(5, 5, 5, 5);
            sp1.Children.Add(btnOk);
        }

        void Box_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            /*
            if (!clicked)
                e.Cancel = true;
            */
        }

        private void input_MouseDown(object sender, MouseEventArgs e)
        {
            if ((sender as TextBox).Text == defaulttext && inputreset == false)
            {
                (sender as TextBox).Text = null;
                inputreset = true;
            }
        }

        void ok_Click(object sender, RoutedEventArgs e)
        {
            clicked = true;
            if (input.Text == defaulttext || input.Text == "")
                MessageBox.Show(errormessage, errortitle);
            else
            {
                Box.Close();
            }
            clicked = false;
        }

        public string ShowDialog()
        {
            Box.ShowDialog();
            return input.Text;
        }
    }
}
