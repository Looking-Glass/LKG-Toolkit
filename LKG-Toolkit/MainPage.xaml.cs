using System.Net;

namespace LKG_Toolkit;

public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		HttpClient client = new HttpClient();
		HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://localhost:33334/encode");
		request.Content = new StringContent(
"""
{
    "calibration" : "D:\\LKG_calibration\\visual.json",
    "source" : "C:\\Users\\zinsl\\Downloads\\tunnel_8K_qs5x9(1).mp4",
    "destination" : "C:\\temp\\prelenticular.mp4",
    "encoder" : "h265",
    "crf" : "12",
    "show_window" : "true"
}
"""
            );
        var resp = client.Send(request);
        text.Text = resp.ToString() + "\n" + resp.Content.ReadAsStringAsync().Result;
    }
}

