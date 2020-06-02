using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace Wareways.PortalProv.Infraestructura.Componentes
{
    public static class HtmlExtensionsTextBox
    {
        public static MvcHtmlString BootstrapTextBoxFor<Tmodel , TValue>(
            this HtmlHelper<Tmodel> htmlHelper,
            Expression<Func<Tmodel, TValue>> expression,
            object htmlAttributes = null
            )
        {
            RouteValueDictionary rvd;
            rvd = new RouteValueDictionary(
                    HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

            return InputExtensions.TextBoxFor(htmlHelper, expression, rvd);
        }
    }
}