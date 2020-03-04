// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace DbLocalizationProvider.AdminUI.Infrastructure
{
    /// <summary>
    /// This class apparently was required in order to get JSON post working in Mvc.
    /// Should be working out of the box - but something is wrong and could not get it into the pipeline.
    /// </summary>
    public class RawBodyBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            using (var s = new StreamReader(controllerContext.HttpContext.Request.InputStream))
            {
                s.BaseStream.Position = 0;
                var content = s.ReadToEnd();

                return JsonConvert.DeserializeObject(content, bindingContext.ModelType);
            }
        }
    }
}
