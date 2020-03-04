// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using DbLocalizationProvider.Commands;

namespace DbLocalizationProvider.AdminUI
{
    public class Events
    {
        /// <summary>
        /// If you need to be so curious and capture moments when new resource is created - you can use this callback
        /// </summary>
        public event CreateNewResources.EventHandler OnNewResourceCreated;

        internal void InvokeNewResourceCreated(string key)
        {
            OnNewResourceCreated?.Invoke(new CreateNewResources.EventArgs(key));
        }
    }
}
