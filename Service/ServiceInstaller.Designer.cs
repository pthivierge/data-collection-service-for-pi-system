namespace DCS.Service
{
    partial class ServiceInstaller
    {
        /// <summary>
        /// Variable n�cessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilis�es.
        /// </summary>
        /// <param name="disposing">true si les ressources manag�es doivent �tre supprim�es�; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code g�n�r� par le Concepteur de composants

        /// <summary>
        /// M�thode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette m�thode avec l'�diteur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalService;
            this.serviceProcessInstaller1.Password = null;
            this.serviceProcessInstaller1.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.Description = "Service that collects data and stores it into the PI System";
            this.serviceInstaller1.DisplayName = "Data Collection Service for the PI System";
            this.serviceInstaller1.ServiceName = "DCSService";
            // 
            // ServiceInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.serviceInstaller1});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;
    }
}
