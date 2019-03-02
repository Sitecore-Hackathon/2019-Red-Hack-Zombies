using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Applications.Dialogs.ItemLister;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using System;

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
            var args = new ClientPipelineArgs();
            args.Parameters["id"] = commandContext.Items[0].ID.Guid.ToString("B").ToUpper();
            args.Parameters["templateId"] = commandContext.Items[0].TemplateID.Guid.ToString("B").ToUpper();
            args.Parameters["path"] = commandContext.Items[0].Paths.FullPath;

            Context.ClientPage.Start(this, "Run", args);
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
                    //For template filtering
                    //IncludeTemplatesForSelection = SelectItemOptions.GetTemplateList(Context.ContentDatabase, new[] { templateId })
                };

                //Show popup for source item selection
                SheerResponse.ShowModalDialog(options.ToUrlString().ToString(), true);
                args.WaitForPostBack(true);
            }
            else
            {
                var selectedSourceItem = Client.GetItemNotNull(new ID(args.Result));
                Assert.ArgumentNotNull(selectedSourceItem, "selectedSourceItem");

                var currentItem = Client.GetItemNotNull(new ID(itemId));
                Assert.ArgumentNotNull(currentItem, "currentItem");

                var isError = false;
                CloneMe(currentItem, selectedSourceItem, out isError);
                Context.ClientPage.ClientResponse.Alert(Translate.Text(itemPath + " succesfully cloned from " + selectedSourceItem.Paths.FullPath));
            }
        }

        /// <summary>
        /// Clones the selectedSourceItem to currentItem
        /// </summary>
        /// <param name="currentItem">Current item.</param>
        /// <param name="selectedSourceItem">Selected new source item.</param>
        private void CloneMe(Item currentItem, Item selectedSourceItem, out bool isError)
        {
            isError = false;

            try
            {
                //Temporary clones path
                var tmpClonePathId = "{4982D011-6BC2-40DD-BBDE-8B2A07B63853}";
                using (new Sitecore.SecurityModel.SecurityDisabler())
                {
                    //Getting temporary clone path item from config
                    var tmpClonesPathItem = Client.GetItemNotNull(tmpClonePathId);
                    if (tmpClonesPathItem == null)
                    {
                        isError = true;
                        Context.ClientPage.ClientResponse.Alert(Translate.Text(string.Format("Temporary clone path with ID: {0} not found",
                            tmpClonePathId)));
                        return;
                    }

                    //Added null check for currentItem
                    if (currentItem == null)
                    {
                        isError = true;
                        Context.ClientPage.ClientResponse.Alert(Translate.Text("Error processing current item. Please try again"));
                        return;
                    }

                    //Added null check for current item's parent
                    if (currentItem.Parent == null)
                    {
                        isError = true;
                        Context.ClientPage.ClientResponse.Alert(Translate.Text(string.Format("Current item parent with ID: {0} not found",
                            currentItem.ParentID.Guid.ToString("B").ToUpper())));
                        return;
                    }

                    //Storing current item's id, name and parent name
                    var currentItemId = currentItem.ID;
                    var currentItemName = currentItem.Name;
                    var currentItemParent = currentItem.Parent;

                    //Creating the temporary clone from the source item in the tmp path
                    var tmpCloneItem = selectedSourceItem.CloneTo(tmpClonesPathItem, false);
                    if (tmpCloneItem == null)
                    {
                        isError = true;
                        Context.ClientPage.ClientResponse.Alert(Translate.Text(string.Format("Couldn't changed the item to a clone. " +
                            "Please check the tmp folder: {0}", tmpClonesPathItem.Paths.FullPath)));
                        return;
                    }

                    //Getting children of current item
                    if (currentItem.Children != null && currentItem.Children.Count > 0)
                    {
                        //Moving all the children of current item to the newly created tmpClone item
                        foreach (Item currentChild in currentItem.Children)
                        {
                            currentChild.MoveTo(tmpCloneItem);
                        }
                    }
                    
                    //Delete current item
                    DeleteItem(currentItem);

                    //Copying tmpClone item to the current item's location with the latter's sitecore id
                    tmpCloneItem.CopyTo(currentItemParent, currentItemName, currentItemId, true);

                    //Clearing all caches
                    Sitecore.Caching.CacheManager.ClearAllCaches();

                    //Getting the current item which has now been made to a clone
                    var newCurrentItem = Client.GetItemNotNull(currentItemId);
                    if (newCurrentItem == null)
                    {
                        isError = true;
                        Context.ClientPage.ClientResponse.Alert(Translate.Text(string.Format("Couldn't changed the item to a clone. " +
                            "Please check the tmp folder: {0} for your original item and its children", tmpClonesPathItem.Paths.FullPath)));
                        return;
                    }

                    //Moving all the children of the current item back to their original location
                    if (tmpCloneItem.Children != null && tmpCloneItem.Children.Count > 0)
                    {
                        foreach (Item tempCloneChild in tmpCloneItem.Children)
                        {
                            tempCloneChild.MoveTo(newCurrentItem);
                        }
                    }

                    //Updating links database for currentItem and selectedSourceItem
                    Globals.LinkDatabase.UpdateReferences(newCurrentItem);
                    Globals.LinkDatabase.UpdateItemVersionReferences(newCurrentItem);
                    Globals.LinkDatabase.UpdateReferences(selectedSourceItem);
                    Globals.LinkDatabase.UpdateItemVersionReferences(selectedSourceItem);

                    //Clearing tmp clone path
                    if (tmpClonesPathItem.Children != null && tmpClonesPathItem.Children.Count > 0)
                    {
                        foreach (Item tmpClone in tmpClonesPathItem.Children)
                        {
                            DeleteItem(tmpClone);
                        }
                    }
                }

                //If everything executes as expected put the isError flag as false
                isError = false;
            }
            catch (Exception ex)
            {
                isError = true;
                Context.ClientPage.ClientResponse.Alert(Translate.Text("Couldn't change the item to a clone. Exception message: " + ex.Message));
                //Logger.Error("Not able to create clone. " + ex.Message, this);
            }
        }

        /// <summary>
        /// Deletes an item.
        /// </summary>
        /// <param name="toBeDeletedItem">Item to be deleted.</param>
        /// <returns>void</returns>
        private void DeleteItem(Item toBeDeletedItem)
        {
            try
            {
                if (toBeDeletedItem == null)
                    throw new Exception("Exception in deleting the item as it is null");

                using (new Sitecore.SecurityModel.SecurityDisabler())
                {
                    if (Sitecore.Configuration.Settings.RecycleBinActive)
                        toBeDeletedItem.Recycle();
                    else
                        toBeDeletedItem.Delete();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
