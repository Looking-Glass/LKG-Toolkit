using System.Net;
using Microsoft.Win32;

namespace LKG_Toolkit;

public partial class MainPage : ContentPage
{
    LKG.Display[] displays;
    int selectedDisp = 0;

	public MainPage()
	{
		InitializeComponent();

        LKG.ServiceConnection.setResponseCallback(GetResponse);
        LKG.ServiceConnection.setAlertCallback(GetAlert);
	}

    private void PageLoaded(object sender, EventArgs e)
    {
        displays = LKG.ServiceConnection.GetDisplays();

        devices.Children.Clear();

        if(displays.Length != 0)
        {
            foreach (var disp in displays)
            {
                Button displayButton = new Button();
                displayButton.Text = disp.hardwareInfo.hwid;
                displayButton.Pressed += (object sender, EventArgs e) =>
                {
                    for (int i = 0; i < devices.Children.Count; i++)
                    {
                        IView c = devices.Children[i];

                        if (c is Button)
                        {
                            ((Button)c).BackgroundColor = Color.FromRgb(198, 196, 211);
                        }

                        if(c == sender)
                        {
                            selectedDisp = i;
                        }
                    }

                    ((Button)sender).BackgroundColor = Color.FromRgb(23, 22, 36);
                    deviceInfo.Text = disp.getInfoString();
                };

                if(devices.Children.Count == 0)
                {
                    selectedDisp = 0;
                    displayButton.BackgroundColor = Color.FromRgb(23, 22, 36);
                    deviceInfo.Text = disp.getInfoString();
                }

                devices.Add(displayButton);
            }

        }
    }

    private void GetResponse(string response)
    {
        text.Text += ("\n" + response);
    }

    private void GetAlert(string error)
    {
        this.DisplayAlert("Error", error, "OK");
    }

    private void Save_Cal_Clicked(object sender, EventArgs e)
    {
        GetAlert("Not Implemented");
    }
    private void Show_Selected(object sender, EventArgs e)
    {
        LKG.ServiceConnection.ShowWindow(selectedDisp);
    }

    private void Hide_Selected(object sender, EventArgs e)
    {
        LKG.ServiceConnection.HideWindow(selectedDisp);
    }

    private async void Show_File(object sender, EventArgs e)
    {
        string path = (await FilePicker.PickAsync())?.FullPath;
        LKG.ServiceConnection.ShowFile(selectedDisp, path);
    }

    private void OnCounterClicked(object sender, EventArgs e)
	{



        //        HttpClient client = new HttpClient();
        //        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://localhost:33334/show");
        //        request.Content = new StringContent(
        //"""
        //        {
        //            "targetDisplay" : 1,
        //            "source" : "C:\\Users\\zinsl\\Downloads\\tunnel_8K_qs5x9(1).mp4",
        //        }
        //"""
        //            );

        //var resp = client.Send(request);
        //text.Text = resp.ToString() + "\n" + resp.Content.ReadAsStringAsync().Result;

//        HttpClient client = new HttpClient();
//        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://localhost:33334/show");
//        request.Content = new StringContent(
//"""
//                {
//                    "targetDisplay" : 1,
//                }
//"""
//            );

//        var resp = client.Send(request);
//        text.Text = resp.ToString() + "\n" + resp.Content.ReadAsStringAsync().Result;

        //      HttpClient client = new HttpClient();
        //		HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://localhost:33334/encode");
        //		request.Content = new StringContent(
        //"""
        //{
        //    "calibration" : "D:\\LKG_calibration\\visual.json",
        //    "source" : "C:\\Users\\zinsl\\Downloads\\tunnel_8K_qs5x9(1).mp4",
        //    "destination" : "C:\\temp\\prelenticular.mp4",
        //    "encoder" : "h265",
        //    "crf" : "12",
        //    "show_window" : "true"
        //}
        //"""
        //            );

        //       var resp = client.Send(request);
        //       text.Text = resp.ToString() + "\n" + resp.Content.ReadAsStringAsync().Result;
    }
}

