<%@ Control 
    Language="C#" Inherits="System.Web.Mvc.ViewUserControl<DropDownListViewModel>" 
%>
<%= Html.DropDownListFor(x => x.SelectedValue, Model.Items) %>