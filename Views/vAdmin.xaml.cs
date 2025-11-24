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

    private void Control_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vControl());
    }
}