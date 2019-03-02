using Sitecore;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Applications.Dialogs.ItemLister;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace CloneMe.Foundation
{
    /// <summary>
    /// CloneMe.Foundation.CloneMeCommands command class
    /// </summary>
    public class CloneMeCommands : Command
    {
        /// <summary>
        /// Executes the code on call from CloneMe button
        /// </summary>
        /// <param name="commandContext">commandContext</param>
        public override void Execute(CommandContext commandContext)
        {
            try
            {
                var args = new ClientPipelineArgs();
                args.Parameters["id"] = commandContext.Items[0].ID.Guid.ToString("B").ToUpper();
                args.Parameters["templateId"] = commandContext.Items[0].TemplateID.Guid.ToString("B").ToUpper();
                args.Parameters["path"] = commandContext.Items[0].Paths.FullPath;

                Context.ClientPage.Start(this, "Run", args);
            }
            catch (System.Exception ex)
            {
                Log.Error("Couldn't change the item to a clone. Exception message: " + ex.Message, this);
                Context.ClientPage.ClientResponse.Alert(Translate.Text("Couldn't change the item to a clone. Exception message: " + 
                                                        ex.Message));
            }
        }

        /// <summary>
        /// CloneMe run method
        /// </summary>
        /// <param name="args">Pipeline args.</param>
        protected void Run(ClientPipelineArgs args)
        {
            //Null check for args
            Assert.ArgumentNotNull(args, "args");

            //Current item's sitecore id, template id and name
            var itemId = args.Parameters["id"];
            var templateId = args.Parameters["templateId"];
            var itemPath = args.Parameters["path"];

            if (!args.IsPostBack)
            {
                //Selecting current item's source for cloning
                var options = new SelectItemOptions()
                {
                    Title = "Select Source Item",
                    Text = "Selecting current item's source for cloning",
                    Icon = "Applications/32x32/sync-colored.png",
                    ResultType = SelectItemOptions.DialogResultType.Id,
                    Root = Client.ContentDatabase.GetItem("/sitecore/content")
                };

                //Show popup for source item selection
                Log.Info(string.Format("Cloning process started for item: {0}", itemPath), this);
                SheerResponse.ShowModalDialog(options.ToUrlString().ToString(), true);
                args.WaitForPostBack(true);
            }
            else if (args.HasResult)
            {
                var selectedSourceItem = Client.GetItemNotNull(new ID(args.Result));
                Assert.ArgumentNotNull(selectedSourceItem, "selectedSourceItem");

                var currentItem = Client.GetItemNotNull(new ID(itemId));
                Assert.ArgumentNotNull(currentItem, "currentItem");

                var isError = false;
                var errorMessage = string.Empty;
                Feature.CloneItemFeature.CloneExistingItem(currentItem, selectedSourceItem, out isError, out errorMessage);

                if (!isError)
                {
                    Context.ClientPage.ClientResponse.Alert(Translate.Text(itemPath + " succesfully cloned from "
                        + selectedSourceItem.Paths.FullPath));
                    Log.Info(itemPath + " succesfully cloned from " + selectedSourceItem.Paths.FullPath, this);
                }
                else
                {
                    Log.Error(errorMessage, this);
                }
            }
        }
    }
}
