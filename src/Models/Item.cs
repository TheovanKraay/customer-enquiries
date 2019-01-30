using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace todo.Models
{
    using Microsoft.Azure.Documents;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;


    public class Item
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "First Name")]
        [JsonProperty(PropertyName = "firstname")]
        public string FirstName { get; set; }

        [Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Last Name")]
        [JsonProperty(PropertyName = "lastname")]
        public string LastName { get; set; }

        [Required]
        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }

        [Required]
        [JsonProperty(PropertyName = "enquiry")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string Enquiry { get; set; }

        [JsonProperty(PropertyName = "isComplete")]
        public bool Completed { get; set; }

        [JsonProperty(PropertyName = "gender")]
        public string gender { get; set; }

        [JsonProperty(PropertyName = "Reply")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string Reply { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Select Target Language")]
        public string languages { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Translated Subject")]
        public string TargetSubjectText { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Translated Enquiry")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string TargetText { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = " ")]
        public string Dummy { get; set; }


    }
}

public static class RequiredLabel
{
    public static MvcHtmlString RequiredLabelFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
    {
        var metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);

        string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
        string labelText = metaData.DisplayName ?? metaData.PropertyName ?? htmlFieldName.Split('.').Last();

        metaData.IsRequired = true;
        if (metaData.IsRequired)
            labelText += " <span class=\"required\"/>";

        if (String.IsNullOrEmpty(labelText))
            return MvcHtmlString.Empty;

        var label = new TagBuilder("label");
        label.Attributes.Add("for", helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));

        foreach (System.ComponentModel.PropertyDescriptor prop in TypeDescriptor.GetProperties(htmlAttributes))
        {
            label.MergeAttribute(prop.Name.Replace('_', '-'), prop.GetValue(htmlAttributes).ToString(), true);
        }

        label.InnerHtml = labelText;
        return MvcHtmlString.Create(label.ToString());
    }

}