namespace ProjectTFS
{
    partial class RibbonMenu : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public RibbonMenu()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tab1 = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.refreshBtn = this.Factory.CreateRibbonButton();
            this.compareWithBtn = this.Factory.CreateRibbonButton();
            this.tab1.SuspendLayout();
            this.group1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.group1);
            this.tab1.Label = "TFS";
            this.tab1.Name = "tab1";
            // 
            // group1
            // 
            this.group1.Items.Add(this.refreshBtn);
            this.group1.Items.Add(this.compareWithBtn);
            this.group1.Label = "Sync";
            this.group1.Name = "group1";
            // 
            // refreshBtn
            // 
            this.refreshBtn.Label = "Refresh";
            this.refreshBtn.Name = "refreshBtn";
            this.refreshBtn.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.refreshBtn_Click);
            // 
            // compareWithBtn
            // 
            this.compareWithBtn.Label = "compare with";
            this.compareWithBtn.Name = "compareWithBtn";
            this.compareWithBtn.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.compareWithBtn_Click);
            // 
            // RibbonMenu
            // 
            this.Name = "RibbonMenu";
            this.RibbonType = "Microsoft.Project.Project";
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.RibbonMenu_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton refreshBtn;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton compareWithBtn;
    }

    partial class ThisRibbonCollection
    {
        internal RibbonMenu RibbonMenu
        {
            get { return this.GetRibbon<RibbonMenu>(); }
        }
    }
}
