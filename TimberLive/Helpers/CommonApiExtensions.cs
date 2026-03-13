namespace TimberLive.Helpers;

public static class CommonApiExtensions
{

    extension(ApiService api)
    {

        public async Task SelectEntityAsync(Guid id, bool focus = false, bool follow = false)
        {
            var url = "misc/select/" + Uri.EscapeDataString(id.ToString());

            if (focus)
            {
                url += "?focus=1";

            }
            else if (follow)
            {
                url += "?follow=1";
            }

            await api.GetStringAsync(url);
        }

        public async Task RenameEntityAsync(Guid id, string newName)
        {
            var url = "misc/rename/" + Uri.EscapeDataString(id.ToString()) 
                + "?newName=" + Uri.EscapeDataString(newName);
            await api.GetStringAsync(url);
        }

    }

}
