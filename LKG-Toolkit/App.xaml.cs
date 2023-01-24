namespace LKG_Toolkit;

#if _LINUX
	public static class Program 
	{
		static void Main()
		{
			Console.WriteLine("LKG-Toolkit does not support Linux, please use Toolkit-CLI");
		}	
	}
#endif


public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
