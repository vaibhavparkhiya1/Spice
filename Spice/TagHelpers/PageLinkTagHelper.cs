using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NuGet.Protocol.Core.Types;
using Spice.Models;

namespace Spice.TagHelpers
{
    [HtmlTargetElement("div",Attributes = "page-model")]
    public class PageLinkTagHelper: TagHelper
    {

        private IUrlHelperFactory urlHelperFactory;

        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
           urlHelperFactory= helperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext viewContext { get; set; }

        public PagingInfo PageModel { get; set; }

        public string   PageAction { get; set; }
        public bool PageClassEnabled { get; set; }

        public string PageClass { get; set; }

        public string PageClassNormal { get; set; }

        public string PageClassSelected { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(viewContext);
            TagBuilder result = new TagBuilder("div");

            for(int i = 1; i <= PageModel.totalPage; i++) 
            {
                TagBuilder tag =new TagBuilder("a");
                string Url = PageModel.urlParam.Replace(":", i.ToString());
                tag.Attributes["href"] = Url;
                if (PageClassEnabled)
                {
                    tag.AddCssClass(PageClass);
                    tag.AddCssClass(i == PageModel.CurrentPage ? PageClassSelected: PageClassNormal);
                }
                tag.InnerHtml.Append(i.ToString());
                result.InnerHtml.AppendHtml(tag);

            }
            output.Content.AppendHtml(result.InnerHtml);    

         
        }


    }
}
