using System.Windows;
using Windows.ApplicationModel;

namespace DesktopHost;
/// <summary>
/// Interaction logic for SplashScreen.xaml
/// </summary>
public partial class SplashScreen : Window {
    
    public SplashScreen() {
        InitializeComponent();
        SetCurrentVersionTag();
    }

    public void SetCurrentVersionTag() {

#if DEBUG
        VersionTag.Text = "DEBUG";
#else
        try {
		    var version = Package.Current.Id.Version;
            VersionTag.Text = $"{version.Major}.{version.Minor}.{version.Build}";
        } catch {}
#endif

	}

}
