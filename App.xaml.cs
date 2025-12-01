namespace ParkIsrael_Octavo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage (new Views.vLogin()));
        }

        protected override async void OnStart()
        {
            await Services.FirebaseInitService.InicializarBaseAsync();
        }
    }
}