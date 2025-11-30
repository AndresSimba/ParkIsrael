namespace ParkIsrael_Octavo.Views;

public partial class vControl : ContentPage
{
	public vControl()
	{
		InitializeComponent();
	}

    private void btnScanner_Clicked(object sender, EventArgs e)
    {
		Navigation.PushAsync(new vScanner());
    }
}