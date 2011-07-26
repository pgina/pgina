using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;

using pGina.Shared.Interfaces;

namespace pGina.Configuration
{
    public class PluginPage
    {
        private IPluginBase m_plugin = null;
        private TabPage pluginTab = new TabPage();
        private System.Windows.Forms.GroupBox pluginBox1;
        private System.Windows.Forms.Label pluginDescription;
        private System.Windows.Forms.Label pluginUuid;
        private System.Windows.Forms.TextBox pluginTxtDescription;
        private System.Windows.Forms.TextBox pluginTxtUuid;
        private System.Windows.Forms.GroupBox pluginState;
        private System.Windows.Forms.CheckBox pluginchkSystem;
        private System.Windows.Forms.CheckBox pluginchkUser;
        private System.Windows.Forms.CheckBox pluginchkNotify;
        private System.Windows.Forms.CheckBox pluginchkGateway;
        private System.Windows.Forms.CheckBox pluginchkAuthorize;
        private System.Windows.Forms.CheckBox pluginchkAuthenticate;
        private System.Windows.Forms.CheckBox pluginchkUI;
        private System.Windows.Forms.Button pluginbtnConfigure;

        public TabPage Page
        {
            get { return pluginTab; }
        }        

        public PluginPage(IPluginBase basePlugin)
        {
            m_plugin = basePlugin;
            SetupPage();
        }

        public void Save()
        {
            int mask = 0;
            if (pluginchkAuthenticate.Checked) mask |= (int) Core.PluginLoader.State.AuthenticateEnabled;
            if (pluginchkAuthorize.Checked) mask |= (int)Core.PluginLoader.State.AuthorizeEnabled;
            if (pluginchkGateway.Checked) mask |= (int)Core.PluginLoader.State.GatewayEnabled;
            if (pluginchkNotify.Checked) mask |= (int)Core.PluginLoader.State.NotificationEnabled;
            if (pluginchkSystem.Checked) mask |= (int)Core.PluginLoader.State.SystemSessionEnabled;
            if (pluginchkUI.Checked) mask |= (int)Core.PluginLoader.State.UIEnabled;
            if (pluginchkUser.Checked) mask |= (int)Core.PluginLoader.State.UserSessionEnabled;

            Core.Settings.Get.SetSetting(m_plugin.Uuid.ToString(), mask);
        }

        private void btnConfigureClick(object sender, EventArgs e)
        {
            if (m_plugin is IPluginConfiguration)
            {
                IPluginConfiguration configPlugin = m_plugin as IPluginConfiguration;
                configPlugin.Configure();
            }
        }        

        private void SetupPage()
        {
            this.pluginBox1 = new System.Windows.Forms.GroupBox();
            this.pluginDescription = new System.Windows.Forms.Label();
            this.pluginUuid = new System.Windows.Forms.Label();
            this.pluginTxtDescription = new System.Windows.Forms.TextBox();
            this.pluginTxtUuid = new System.Windows.Forms.TextBox();
            this.pluginState = new System.Windows.Forms.GroupBox();
            this.pluginchkUI = new System.Windows.Forms.CheckBox();
            this.pluginchkAuthenticate = new System.Windows.Forms.CheckBox();
            this.pluginchkAuthorize = new System.Windows.Forms.CheckBox();
            this.pluginchkGateway = new System.Windows.Forms.CheckBox();
            this.pluginchkNotify = new System.Windows.Forms.CheckBox();
            this.pluginchkUser = new System.Windows.Forms.CheckBox();
            this.pluginchkSystem = new System.Windows.Forms.CheckBox();
            this.pluginbtnConfigure = new System.Windows.Forms.Button();
            this.pluginBox1.SuspendLayout();
            this.pluginState.SuspendLayout();

            // 
            // pluginTab
            // 
            this.pluginTab.Controls.Add(this.pluginbtnConfigure);
            this.pluginTab.Controls.Add(this.pluginState);
            this.pluginTab.Controls.Add(this.pluginBox1);
            this.pluginTab.Location = new System.Drawing.Point(4, 22);
            this.pluginTab.Name = "pluginTab";
            this.pluginTab.Padding = new System.Windows.Forms.Padding(3);
            this.pluginTab.Size = new System.Drawing.Size(393, 395);
            this.pluginTab.TabIndex = 2;
            this.pluginTab.Text = m_plugin.Name;
            this.pluginTab.UseVisualStyleBackColor = true;
            // 
            // pluginBox1
            // 
            this.pluginBox1.Controls.Add(this.pluginTxtUuid);
            this.pluginBox1.Controls.Add(this.pluginTxtDescription);
            this.pluginBox1.Controls.Add(this.pluginUuid);
            this.pluginBox1.Controls.Add(this.pluginDescription);
            this.pluginBox1.Location = new System.Drawing.Point(21, 17);
            this.pluginBox1.Name = "pluginBox1";
            this.pluginBox1.Size = new System.Drawing.Size(349, 177);
            this.pluginBox1.TabIndex = 0;
            this.pluginBox1.TabStop = false;
            this.pluginBox1.Text = "Information";
            // 
            // pluginDescription
            // 
            this.pluginDescription.AutoSize = true;
            this.pluginDescription.Location = new System.Drawing.Point(14, 32);
            this.pluginDescription.Name = "pluginDescription";
            this.pluginDescription.Size = new System.Drawing.Size(63, 13);
            this.pluginDescription.TabIndex = 1;
            this.pluginDescription.Text = "Description:";
            // 
            // pluginUuid
            // 
            this.pluginUuid.AutoSize = true;
            this.pluginUuid.Location = new System.Drawing.Point(14, 142);
            this.pluginUuid.Name = "pluginUuid";
            this.pluginUuid.Size = new System.Drawing.Size(32, 13);
            this.pluginUuid.TabIndex = 2;
            this.pluginUuid.Text = "Uuid:";
            // 
            // pluginTxtDescription
            // 
            this.pluginTxtDescription.AcceptsReturn = true;
            this.pluginTxtDescription.AcceptsTab = true;
            this.pluginTxtDescription.Location = new System.Drawing.Point(83, 32);
            this.pluginTxtDescription.Multiline = true;
            this.pluginTxtDescription.Name = "pluginTxtDescription";
            this.pluginTxtDescription.ReadOnly = true;
            this.pluginTxtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.pluginTxtDescription.Size = new System.Drawing.Size(248, 100);
            this.pluginTxtDescription.TabIndex = 3;
            this.pluginTxtDescription.Text = m_plugin.Description;
            // 
            // pluginTxtUuid
            // 
            this.pluginTxtUuid.Location = new System.Drawing.Point(83, 139);
            this.pluginTxtUuid.Name = "pluginTxtUuid";
            this.pluginTxtUuid.ReadOnly = true;
            this.pluginTxtUuid.Size = new System.Drawing.Size(248, 20);
            this.pluginTxtUuid.TabIndex = 4;
            this.pluginTxtUuid.Text = m_plugin.Uuid.ToString();
            // 
            // pluginState
            // 
            this.pluginState.Controls.Add(this.pluginchkSystem);
            this.pluginState.Controls.Add(this.pluginchkUser);
            this.pluginState.Controls.Add(this.pluginchkNotify);
            this.pluginState.Controls.Add(this.pluginchkGateway);
            this.pluginState.Controls.Add(this.pluginchkAuthorize);
            this.pluginState.Controls.Add(this.pluginchkAuthenticate);
            this.pluginState.Controls.Add(this.pluginchkUI);
            this.pluginState.Location = new System.Drawing.Point(21, 201);
            this.pluginState.Name = "pluginState";
            this.pluginState.Size = new System.Drawing.Size(191, 178);
            this.pluginState.TabIndex = 1;
            this.pluginState.TabStop = false;
            this.pluginState.Text = "State";
            // 
            // pluginchkUI
            // 
            this.pluginchkUI.AutoSize = true;
            this.pluginchkUI.Location = new System.Drawing.Point(17, 19);
            this.pluginchkUI.Name = "pluginchkUI";
            this.pluginchkUI.Size = new System.Drawing.Size(145, 17);
            this.pluginchkUI.TabIndex = 0;
            this.pluginchkUI.Text = "Use for Authentication UI";
            this.pluginchkUI.UseVisualStyleBackColor = true;
            this.pluginchkUI.Enabled = (m_plugin is IPluginAuthenticationUI);
            this.pluginchkUI.Checked = Core.PluginLoader.IsEnabledFor<IPluginAuthenticationUI>(m_plugin);            

            // 
            // pluginchkAuthenticate
            // 
            this.pluginchkAuthenticate.AutoSize = true;
            this.pluginchkAuthenticate.Location = new System.Drawing.Point(17, 42);
            this.pluginchkAuthenticate.Name = "pluginchkAuthenticate";
            this.pluginchkAuthenticate.Size = new System.Drawing.Size(131, 17);
            this.pluginchkAuthenticate.TabIndex = 1;
            this.pluginchkAuthenticate.Text = "Use for Authentication";
            this.pluginchkAuthenticate.UseVisualStyleBackColor = true;
            this.pluginchkAuthenticate.Enabled = (m_plugin is IPluginAuthentication);
            this.pluginchkAuthenticate.Checked = Core.PluginLoader.IsEnabledFor<IPluginAuthentication>(m_plugin);            

            // 
            // pluginchkAuthorize
            // 
            this.pluginchkAuthorize.AutoSize = true;
            this.pluginchkAuthorize.Location = new System.Drawing.Point(17, 65);
            this.pluginchkAuthorize.Name = "pluginchkAuthorize";
            this.pluginchkAuthorize.Size = new System.Drawing.Size(124, 17);
            this.pluginchkAuthorize.TabIndex = 2;
            this.pluginchkAuthorize.Text = "Use for Authorization";
            this.pluginchkAuthorize.UseVisualStyleBackColor = true;
            this.pluginchkAuthorize.Enabled = (m_plugin is IPluginAuthorization);
            this.pluginchkAuthorize.Checked = Core.PluginLoader.IsEnabledFor<IPluginAuthorization>(m_plugin);            

            // 
            // pluginchkGateway
            // 
            this.pluginchkGateway.AutoSize = true;
            this.pluginchkGateway.Location = new System.Drawing.Point(17, 88);
            this.pluginchkGateway.Name = "pluginchkGateway";
            this.pluginchkGateway.Size = new System.Drawing.Size(105, 17);
            this.pluginchkGateway.TabIndex = 3;
            this.pluginchkGateway.Text = "Use for Gateway";
            this.pluginchkGateway.UseVisualStyleBackColor = true;
            this.pluginchkGateway.Enabled = (m_plugin is IPluginAuthenticationGateway);
            this.pluginchkGateway.Checked = Core.PluginLoader.IsEnabledFor<IPluginAuthenticationGateway>(m_plugin);            

            // 
            // pluginchkNotify
            // 
            this.pluginchkNotify.AutoSize = true;
            this.pluginchkNotify.Location = new System.Drawing.Point(17, 111);
            this.pluginchkNotify.Name = "pluginchkNotify";
            this.pluginchkNotify.Size = new System.Drawing.Size(121, 17);
            this.pluginchkNotify.TabIndex = 4;
            this.pluginchkNotify.Text = "Use for Notifications";
            this.pluginchkNotify.UseVisualStyleBackColor = true;
            this.pluginchkNotify.Enabled = (m_plugin is IPluginEventNotifications);
            this.pluginchkNotify.Checked = Core.PluginLoader.IsEnabledFor<IPluginEventNotifications>(m_plugin);            

            // 
            // pluginchkUser
            // 
            this.pluginchkUser.AutoSize = true;
            this.pluginchkUser.Location = new System.Drawing.Point(17, 134);
            this.pluginchkUser.Name = "pluginchkUser";
            this.pluginchkUser.Size = new System.Drawing.Size(152, 17);
            this.pluginchkUser.TabIndex = 5;
            this.pluginchkUser.Text = "Use in User Session (User)";
            this.pluginchkUser.UseVisualStyleBackColor = true;
            this.pluginchkUser.Enabled = (m_plugin is IPluginUserSessionHelper);
            this.pluginchkUser.Checked = Core.PluginLoader.IsEnabledFor<IPluginUserSessionHelper>(m_plugin);            

            // 
            // pluginchkSystem
            // 
            this.pluginchkSystem.AutoSize = true;
            this.pluginchkSystem.Location = new System.Drawing.Point(17, 155);
            this.pluginchkSystem.Name = "pluginchkSystem";
            this.pluginchkSystem.Size = new System.Drawing.Size(164, 17);
            this.pluginchkSystem.TabIndex = 6;
            this.pluginchkSystem.Text = "Use in User Session (System)";
            this.pluginchkSystem.UseVisualStyleBackColor = true;
            this.pluginchkSystem.Enabled = (m_plugin is IPluginSystemSessionHelper);
            this.pluginchkSystem.Checked = Core.PluginLoader.IsEnabledFor<IPluginSystemSessionHelper>(m_plugin);            

            // 
            // pluginbtnConfigure
            // 
            this.pluginbtnConfigure.Location = new System.Drawing.Point(257, 353);
            this.pluginbtnConfigure.Name = "pluginbtnConfigure";
            this.pluginbtnConfigure.Size = new System.Drawing.Size(112, 25);
            this.pluginbtnConfigure.TabIndex = 2;
            this.pluginbtnConfigure.Text = "Configure Plugin";
            this.pluginbtnConfigure.UseVisualStyleBackColor = true;
            this.pluginbtnConfigure.Click += new System.EventHandler(this.btnConfigureClick);
            this.pluginbtnConfigure.Enabled = (m_plugin is IPluginConfiguration);

            this.pluginTab.ResumeLayout(false);
            this.pluginBox1.ResumeLayout(false);
            this.pluginBox1.PerformLayout();
            this.pluginState.ResumeLayout(false);
            this.pluginState.PerformLayout();            
        }
    }
}
