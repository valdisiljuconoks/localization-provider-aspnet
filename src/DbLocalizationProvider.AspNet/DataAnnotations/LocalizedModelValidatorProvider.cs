// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using DbLocalizationProvider.Abstractions;

namespace DbLocalizationProvider.DataAnnotations
{
    public class LocalizedModelValidatorProvider : DataAnnotationsModelValidatorProvider
    {
        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes)
        {
            if(metadata.ContainerType == null)
            {
                return base.GetValidators(metadata, context, attributes);
            }

            if(metadata.ContainerType.GetCustomAttribute<LocalizedModelAttribute>() == null)
            {
                return base.GetValidators(metadata, context, attributes);
            }

            foreach (var attribute in attributes.OfType<ValidationAttribute>())
            {
                var resourceKey = ResourceKeyBuilder.BuildResourceKey(metadata.ContainerType, metadata.PropertyName, attribute);
                var translation = AspNet.DataAnnotations.ModelMetadataLocalizationHelper.GetTranslation(resourceKey);
                if(!string.IsNullOrEmpty(translation))
                {
                    attribute.ErrorMessage = translation;
                }
            }

            return base.GetValidators(metadata, context, attributes);
        }
    }
}
