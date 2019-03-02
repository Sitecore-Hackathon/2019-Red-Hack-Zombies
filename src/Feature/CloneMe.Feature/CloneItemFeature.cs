using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using System;
namespace CloneMe.Feature
{
    public static class CloneItemFeature
    {
        /// <summary>
        /// Clones the selectedSourceItem to currentItem
        /// </summary>
        /// <param name="currentItem">Current item.</param>
        /// <param name="selectedSourceItem">Selected new source item.</param>
        public static void CloneExistingItem(Item currentItem, Item selectedSourceItem, out bool isError, out string errorMessage)
        {
            isError = false;
            errorMessage = string.Empty;

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
                        errorMessage = string.Format("Temporary clone path with ID: {0} not found", tmpClonePathId);
                        Context.ClientPage.ClientResponse.Alert(Translate.Text(string.Format("Temporary clone path with ID: {0} not found",
                            tmpClonePathId)));
                        return;
                    }

                    //Added null check for currentItem
                    if (currentItem == null)
                    {
                        isError = true;
                        errorMessage = "Error processing current item. Please try again";
                        Context.ClientPage.ClientResponse.Alert(Translate.Text("Error processing current item. Please try again"));
                        return;
                    }

                    //Added null check for current item's parent
                    if (currentItem.Parent == null)
                    {
                        isError = true;
                        errorMessage = string.Format("Current item parent with ID: {0} not found",
                            currentItem.ParentID.Guid.ToString("B").ToUpper());
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
                        errorMessage = string.Format("Couldn't changed the item to a clone. Please check the tmp folder: {0}",
                            tmpClonesPathItem.Paths.FullPath);
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
                        errorMessage = string.Format("Couldn't changed the item to a clone. Please check the tmp folder: {0} " +
                            "for your original item and its children", tmpClonesPathItem.Paths.FullPath);
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
                errorMessage = "Couldn't change the item to a clone. Exception message: " + ex.Message;
                Context.ClientPage.ClientResponse.Alert(Translate.Text("Couldn't change the item to a clone. Exception message: "
                    + ex.Message));
            }
        }

        /// <summary>
        /// Deletes an item.
        /// </summary>
        /// <param name="toBeDeletedItem">Item to be deleted.</param>
        /// <returns>void</returns>
        private static void DeleteItem(Item toBeDeletedItem)
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
    }
}
