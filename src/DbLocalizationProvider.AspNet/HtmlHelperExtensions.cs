// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using DbLocalizationProvider.Internal;
using ExpressionHelper = DbLocalizationProvider.Internal.ExpressionHelper;

namespace DbLocalizationProvider
{
    /// <summary>
    /// Html helper to help you generate some markup
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Gets the resource translation.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="model">The resource expression.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns>Translation</returns>
        /// <exception cref="ArgumentNullException">model</exception>
        public static string GetString(this HtmlHelper helper, Expression<Func<object>> model, params object[] formatArguments)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));

            return LocalizationProvider.Current.GetStringByCulture(model, CultureInfo.CurrentUICulture, formatArguments);
        }

        /// <summary>
        /// Gets the resource translation.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="model">The resource extension.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns>Translation</returns>
        /// <exception cref="ArgumentNullException">model</exception>
        public static MvcHtmlString Translate(this HtmlHelper helper, Expression<Func<object>> model, params object[] formatArguments)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));

            return new MvcHtmlString(LocalizationProvider.Current.GetStringByCulture(model, CultureInfo.CurrentUICulture, formatArguments));
        }

        /// <summary>
        /// Gets the resource translation.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="model">The resource expression.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// model
        /// or
        /// culture
        /// </exception>
        public static MvcHtmlString TranslateByCulture(this HtmlHelper helper, Expression<Func<object>> model, CultureInfo culture, params object[] formatArguments)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));
            if(culture == null) throw new ArgumentNullException(nameof(culture));

            return new MvcHtmlString(LocalizationProvider.Current.GetStringByCulture(model, culture, formatArguments));
        }

        /// <summary>
        /// Gets the resource translation.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="model">The resource expression.</param>
        /// <param name="customAttribute">The custom attribute.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns></returns>
        public static MvcHtmlString Translate(this HtmlHelper helper, Expression<Func<object>> model, Type customAttribute, params object[] formatArguments)
        {
            return TranslateByCulture(helper, model, customAttribute, CultureInfo.CurrentUICulture, formatArguments);
        }

        /// <summary>
        /// Gets the resource translation by language.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="model">The resource expression.</param>
        /// <param name="customAttribute">The custom attribute.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns>Translation</returns>
        /// <exception cref="ArgumentNullException">
        /// model
        /// or
        /// customAttribute
        /// or
        /// culture
        /// </exception>
        /// <exception cref="ArgumentException">Given type `{customAttribute.FullName}` is not of type `System.Attribute`</exception>
        public static MvcHtmlString TranslateByCulture(this HtmlHelper helper, Expression<Func<object>> model, Type customAttribute, CultureInfo culture, params object[] formatArguments)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));
            if(customAttribute == null) throw new ArgumentNullException(nameof(customAttribute));
            if(culture == null) throw new ArgumentNullException(nameof(culture));
            if(!typeof(Attribute).IsAssignableFrom(customAttribute)) throw new ArgumentException($"Given type `{customAttribute.FullName}` is not of type `System.Attribute`");

            var resourceKey = ResourceKeyBuilder.BuildResourceKey(ExpressionHelper.GetFullMemberName(model), customAttribute);

            return new MvcHtmlString(LocalizationProvider.Current.GetStringByCulture(resourceKey, culture, formatArguments));
        }

        /// <summary>
        /// Gets the resource translation.
        /// </summary>
        /// <typeparam name="TModel">The type of the resource.</typeparam>
        /// <typeparam name="TValue">The type of the translation.</typeparam>
        /// <param name="html">The helper.</param>
        /// <param name="expression">The resource expression.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns>Translations</returns>
        public static MvcHtmlString TranslateFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, params object[] formatArguments)
        {
            return TranslateForByCulture(html, expression, CultureInfo.CurrentUICulture, formatArguments);
        }

        /// <summary>
        /// Gets the resource translation.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The helper.</param>
        /// <param name="expression">The resource expression.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns>Translation</returns>
        public static MvcHtmlString TranslateForByCulture<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, CultureInfo culture, params object[] formatArguments)
        {
            return new MvcHtmlString(LocalizationProvider.Current.GetStringByCulture(ExpressionHelper.GetFullMemberName(expression), culture, formatArguments));
        }

        /// <summary>
        /// Gets the Enum resource translation.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="target">The target enumeration type.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns>Translated enum</returns>
        public static MvcHtmlString Translate(this HtmlHelper helper, Enum target, params object[] formatArguments)
        {
            return TranslateByCulture(helper, target, CultureInfo.CurrentUICulture, formatArguments);
        }

        /// <summary>
        /// Translates resource enum the by culture.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="target">The target enumeration type.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns>Translated enum</returns>
        public static MvcHtmlString TranslateByCulture(this HtmlHelper helper, Enum target, CultureInfo culture, params object[] formatArguments)
        {
            return new MvcHtmlString(target.TranslateByCulture(culture, formatArguments));
        }

        /// <summary>
        /// Translates the specified enum.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="helper">The helper.</param>
        /// <param name="target">The target.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns>Translated enum</returns>
        public static MvcHtmlString Translate<TModel>(this HtmlHelper<TModel> helper, Enum target, params object[] formatArguments)
        {
            return new MvcHtmlString(target.Translate(formatArguments));
        }

        /// <summary>
        /// Translates enum by culture.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="helper">The helper.</param>
        /// <param name="target">The target enum.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns></returns>
        public static MvcHtmlString TranslateByCulture<TModel>(this HtmlHelper<TModel> helper, Enum target, CultureInfo culture, params object[] formatArguments)
        {
            return new MvcHtmlString(target.TranslateByCulture(culture, formatArguments));
        }

        /// <summary>
        /// Translates view model property.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="customAttribute">The custom attribute.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns></returns>
        public static MvcHtmlString TranslateFor<TModel, TValue>(this HtmlHelper<TModel> html,
                                                                 Expression<Func<TModel, TValue>> expression,
                                                                 Type customAttribute,
                                                                 params object[] formatArguments)
        {
            return TranslateForByCulture(html, expression, customAttribute, CultureInfo.CurrentUICulture, formatArguments);
        }

        /// <summary>
        /// Translates view model property by culture.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The helper.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="customAttribute">The custom attribute.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="formatArguments">The format arguments.</param>
        /// <returns>Translation</returns>
        /// <exception cref="ArgumentNullException">
        /// customAttribute
        /// or
        /// culture
        /// </exception>
        /// <exception cref="ArgumentException">Given type `{customAttribute.FullName}` is not of type `System.Attribute`</exception>
        public static MvcHtmlString TranslateForByCulture<TModel, TValue>(this HtmlHelper<TModel> html,
                                                                 Expression<Func<TModel, TValue>> expression,
                                                                 Type customAttribute,
                                                                 CultureInfo culture,
                                                                 params object[] formatArguments)
        {
            if(customAttribute == null) throw new ArgumentNullException(nameof(customAttribute));
            if(culture == null) throw new ArgumentNullException(nameof(culture));
            if(!typeof(Attribute).IsAssignableFrom(customAttribute)) throw new ArgumentException($"Given type `{customAttribute.FullName}` is not of type `System.Attribute`");

            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            var pi = metadata.ContainerType.GetProperty(metadata.PropertyName);
            if(pi != null)
            {
                if(pi.GetCustomAttribute(customAttribute) == null) return MvcHtmlString.Empty;
            }

            return new MvcHtmlString(LocalizationProvider.Current.GetStringByCulture(ResourceKeyBuilder.BuildResourceKey(metadata.ContainerType,
                                                                                                                         metadata.PropertyName,
                                                                                                                         customAttribute),
                                                                                     culture,
                                                                                     formatArguments));
        }
    }
}
