using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace MultipleThreadUiTest1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        static Thread calculateThread;
        private static EventWaitHandle waitHandle = new ManualResetEvent(initialState: true);

        public MainWindow()
        {
            InitializeComponent();
        }

        public void initialProgressBar(int start, int end)
        {
            ProgressBar_Progress.Minimum = start;
            ProgressBar_Progress.Maximum = end;
        }

        public void calculate(int start, int end)
        {
            string result = "";

            for (int i = start; i <= end; i++)
            {
                //判断该数字是否是质数
                bool isPrime = false;
                //阻止当前线程，直到当前 WaitHandle 收到信号。
                //不放在内层循环，是因为每次都检测的话，速度会变慢很多。
                waitHandle.WaitOne();

                for (int j = 2; j < i; j++)
                {

                    if (i % j == 0)
                    {
                        isPrime = true;
                        break;
                    }

                }
                if (isPrime == false)
                {
                    result += i.ToString() + "; ";
                    //只把更新UI的操作放到排程器中，让UI线程去执行。其他的运算都放到外面，让该线程自己独立执行。
                    //否则过多的UI线程运算会造成界面卡顿。
                    this.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            TextBlock_Message.Text = i.ToString();
                            ProgressBar_Progress.Value = i;
                        }
                        ));
                }
            }
            this.Dispatcher.Invoke(new Action(
                delegate
                {
                    ProgressBar_Progress.Value = end;
                    TextBox_Result.Text = result;
                }));
        }


        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            
            int start = Int32.Parse(TextBox_StartNum.Text);
            int end = Int32.Parse(TextBox_EndNum.Text);
            initialProgressBar(start, end);

            calculateThread = new Thread(() => calculate(start, end));
            calculateThread.IsBackground = true;
            calculateThread.Start();

        }

        private void Button_Pause_Click(object sender, RoutedEventArgs e)
        {
            //检查waitHandle的事件状态
            if (waitHandle.WaitOne(0))
            {
                //将事件状态设置为非终止，从而导致线程受阻。
                waitHandle.Reset();
            }
            else
            {
                //将事件状态设置为有信号，从而允许一个或多个等待线程继续执行。
                waitHandle.Set();
            }
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            //Todo:
            //停止这里有问题，停止之后，dispatch仍然执行一次，ui又被刷新了。
            waitHandle.Reset();
            waitHandle.Set();
            calculateThread.Abort();
            ProgressBar_Progress.Value = 0;
            TextBlock_Message.Text = "寻找质数";
            TextBox_StartNum.Text = "";
            TextBox_EndNum.Text = "";
            TextBox_Result.Text = "";
        }
    }
}
