using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPEmulatorTest
{
    public class ListHelper
    {
        public ListItem AddItem(ClientContext context, string title)
        {
            var item = context.Web.Lists.GetByTitle("Test").AddItem(new ListItemCreationInformation());
            item["Title"] = title;
            item.Update();            
            context.ExecuteQuery();

            var items = context.Web.Lists.GetByTitle("Test").GetItems(new CamlQuery());
            var added = items.GetById(0);
            context.Load(added);
            return added;
        }
    }
}
