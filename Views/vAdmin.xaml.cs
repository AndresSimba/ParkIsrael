namespace ParkIsrael_Octavo.Views;

public partial class vAdmin : ContentPage
{
	public vAdmin()
	{
		InitializeComponent();
	}

    private void btnAdmin_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync (new vInformacion());
    }

    private void btnControl_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vControl());
    }

    private void btnGestion_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vGestion());
    }
}