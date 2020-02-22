// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

namespace DbLocalizationProvider.AdminUI
{
    /// <summary>
    /// Context to describe who is using AdminUI
    /// </summary>
    public enum UiContextMode
    {
        /// <summary>
        /// Someone who is unknown
        /// </summary>
        None,

        /// <summary>
        /// In this mode editor is trying to do his/her job
        /// </summary>
        Edit,

        /// <summary>
        /// Someone with higher privileges is messing around in AdminUI
        /// </summary>
        Admin
    }
}
