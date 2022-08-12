using System.Net;
using Microsoft.Win32;

namespace LKG_Toolkit;

public partial class MainPage : ContentPage
{
    LKG.Display[] displays;
    int selectedDisp = 0;

    string selectedFilePath;

	public MainPage()
	{
		InitializeComponent();

        LKG.ServiceConnection.setResponseCallback(GetResponse);
        LKG.ServiceConnection.setAlertCallback(GetAlert);
        LKG.ServiceConnection.setPollCallback(GetUpdateCallback);
        LKG.ServiceConnection.setConnectionStateCallback(GetConnectionState);
	}

    private void UpdateDevicePicker()
    {
        devices.Children.Clear();

        if (displays.Length != 0)
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

                        if (c == sender)
                        {
                            selectedDisp = i;
                        }
                    }

                    ((Button)sender).BackgroundColor = Color.FromRgb(23, 22, 36);
                    deviceInfo.Text = disp.getInfoString();
                };

                if (devices.Children.Count == 0)
                {
                    selectedDisp = 0;
                    displayButton.BackgroundColor = Color.FromRgb(23, 22, 36);
                    deviceInfo.Text = disp.getInfoString();
                }

                devices.Add(displayButton);
            }

        }
    }

    private void UpdateFilePreview()
    {
        if (selectedFilePath != null && selectedFilePath != "")
        {
            ImagePreview.Source = selectedFilePath;
            fileLabel.Text = selectedFilePath;
        }
    }

    private void GetConnectionState(bool state)
    {
        if (state)
        {
            stateLabel.BackgroundColor = Color.FromRgb(0, 255, 0);
            stateLabel.TextColor = Color.FromRgb(0, 0, 0);
            stateLabel.Text = "Connection State: Connected";
        }
        else
        {
            stateLabel.BackgroundColor = Color.FromRgb(255, 0, 0);
            stateLabel.TextColor = Color.FromRgb(0, 0, 0);
            stateLabel.Text = "Connection State: Disconnected";
        }
    }

    private void GetUpdateCallback(LKG.Display[] displays)
    {
        this.displays = displays;
        UpdateDevicePicker();
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
        if(selectedFilePath == null || selectedFilePath == "")
        {
            selectedFilePath = (await FilePicker.PickAsync())?.FullPath;
            UpdateFilePreview();
        }

        LKG.ServiceConnection.ShowFile(selectedDisp, selectedFilePath);
    }

    private async void FileSelect_Clicked(object sender, EventArgs e)
    {
        selectedFilePath = (await FilePicker.PickAsync())?.FullPath;
        UpdateFilePreview();
    }
}

