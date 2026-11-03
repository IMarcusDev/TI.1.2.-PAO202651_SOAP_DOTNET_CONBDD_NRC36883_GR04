using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace MOV_EUREKABANK_DOTNET_SOAP
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();
            UserAppTheme = AppTheme.Light;
            // Con el teclado abierto, la pantalla se reduce para que el formulario siga visible (adjustResize).
            On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
