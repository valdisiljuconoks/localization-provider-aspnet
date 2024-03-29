// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using DbLocalizationProvider.Abstractions;

namespace DbLocalizationProvider.AspNet.DataAnnotations
{
    public class LocalizedMetadataProvider : DataAnnotationsModelMetadataProvider
    {
        protected override ModelMetadata CreateMetadata(
            IEnumerable<Attribute> attributes,
            Type containerType,
            Func<object> modelAccessor,
            Type modelType,
            string propertyName)
        {
            var theAttributes = attributes.ToList();
            var data = base.CreateMetadata(theAttributes, containerType, modelAccessor, modelType, propertyName);

            if (containerType == null) return data;
            if (containerType.GetCustomAttribute<LocalizedModelAttribute>() == null) return data;

            data.DisplayName = data.DisplayName != null && !ConfigurationContext.Current.ResourceLookupFilter(data.DisplayName)
                ? AspNet.DataAnnotations.ModelMetadataLocalizationHelper.GetTranslation(data.DisplayName)
                : AspNet.DataAnnotations.ModelMetadataLocalizationHelper.GetTranslation(containerType, propertyName);

            // TODO: extract this as decorator
            if (data.IsRequired
                && ConfigurationContext.Current.ModelMetadataProviders.MarkRequiredFields
                && ConfigurationContext.Current.ModelMetadataProviders.RequiredFieldResource != null)
            {
                data.DisplayName += LocalizationProvider.Current.GetStringByCulture(
                    ConfigurationContext.Current.ModelMetadataProviders.RequiredFieldResource,
                    CultureInfo.CurrentUICulture);
            }

            var displayAttribute = theAttributes.OfType<DisplayAttribute>().FirstOrDefault();
            if (displayAttribute?.Description != null)
            {
                data.Description = AspNet.DataAnnotations.ModelMetadataLocalizationHelper.GetTranslation(containerType, $"{propertyName}-Description");
            }

            return data;
        }
    }
}
