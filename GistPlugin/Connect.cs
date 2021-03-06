namespace GistPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EnvDTE;

    using EnvDTE80;

    using Extensibility;

    using Microsoft.VisualStudio.CommandBars;
    using System.IO;

    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        private AddIn _addInInstance;
        private DTE2 _applicationObject;
        private const string GIST_THIS_CODE = "GistThisCode";
        private const string SET_GISTID = "SetGistID";
        #region IDTCommandTarget Members





        #endregion

        #region IDTExtensibility2 Members

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;
            Array empty = null;
            if (connectMode == ext_ConnectMode.ext_cm_AfterStartup)
            {
                OnStartupComplete(ref empty);
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom) { }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom) { }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {

            Command codeWindowCommand;
            codeWindowCommand = AddCommand(GIST_THIS_CODE, "Send to Gist", 59);
            CommandBarControl _codeWindowButton;
            var commandBars = (CommandBars)_applicationObject.CommandBars;
            CommandBar commandBar = commandBars["Code Window"];
            _codeWindowButton = AddCommandBar(codeWindowCommand,
                commandBar,
                1,//commandBar.Controls.Count + 1,
                "Send to Gist",
                "Send to Gist");
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom) { }

        #endregion

        public CommandBarControl AddCommandBar(Command cmd, CommandBar parent, int pos, string caption, string tooltip)
        {
            var newControl = (CommandBarControl)cmd.AddControl(parent, pos);
            newControl.Caption = caption;
            newControl.TooltipText = tooltip;
            return newControl;
        }


        public Command AddCommand(string name, string caption, int iconID)
        {
            Command foundCommand = null;

            try
            {
                foundCommand = _applicationObject.Commands.Item(_addInInstance.ProgID + "." + name);
            }
            catch (Exception)
            {

            }
            var empty = new object[0];
            if (foundCommand == null)
            {
                foundCommand = _applicationObject.Commands.AddNamedCommand(
                    _addInInstance,
                    name,
                    name,
                    caption,
                    true,
                    iconID,
                    ref empty, 1 | 2);
            }
            return foundCommand;
        }
        public void Exec(string CmdName, vsCommandExecOption ExecuteOption, ref object VariantIn, ref object VariantOut,
                         ref bool Handled)
        {
            if (ExecuteOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                string prog = _addInInstance.ProgID + "." + GIST_THIS_CODE;
                if (CmdName == prog)
                {
                    var selection = (TextSelection)_applicationObject.ActiveWindow.Document.Selection;
                    if (selection.Text != String.Empty)
                    {
                        selection.SetBookmark();
                        var guid = Guid.NewGuid().ToString();
                        var initMark = String.Format("//GIST:{0}", guid);
                        var endMark = String.Format("//GISTEND:{0}", guid);
                        var newBlockBuilder = new System.Text.StringBuilder();
                        newBlockBuilder.AppendLine(initMark);
                        newBlockBuilder.AppendLine(selection.Text);
                        newBlockBuilder.AppendLine(endMark);
                        selection.Copy();
                        selection.Text = newBlockBuilder.ToString();
                        _applicationObject.ItemOperations.Navigate("gist.github.com", vsNavigateOptions.vsNavigateOptionsNewWindow);
                    }

                }
            }
        }

        public void QueryStatus(string CmdName, vsCommandStatusTextWanted NeededText, ref vsCommandStatus StatusOption,
                               ref object CommandText)
        {

            if (CmdName == _addInInstance.ProgID + "." + GIST_THIS_CODE)
            {
                StatusOption = (vsCommandStatus)(vsCommandStatus.vsCommandStatusSupported +
                               (int)vsCommandStatus.vsCommandStatusEnabled);

            }
            else
            {
                StatusOption = vsCommandStatus.vsCommandStatusUnsupported;
            }


        }


    }
}