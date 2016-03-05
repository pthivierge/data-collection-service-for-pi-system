using System.ComponentModel;
using System.Configuration.Install;

namespace WSR.Service
{
    [RunInstaller(true)]
    public partial class ServiceInstaller : Installer
    {
        public ServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
