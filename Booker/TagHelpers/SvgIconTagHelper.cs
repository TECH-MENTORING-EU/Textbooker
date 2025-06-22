using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Xml.Linq;

namespace Booker.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("svg", Attributes = "icon")]
    public class SvgIconTagHelper : TagHelper
    {
        private static readonly XDocument xDoc = XDocument.Load("wwwroot/img/icons.svg");

        public required string Icon { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            XNamespace aw = xDoc.Root?.Attribute("xmlns")?.Value ?? "http://www.w3.org/2000/svg";

            var symbol = xDoc.Descendants(aw + "symbol")
                 .FirstOrDefault(el => el.Attribute("id")?.Value == Icon);
            var viewBox = symbol?.Attribute("viewBox")?.Value;

            if (viewBox == null)
            {
                throw new InvalidOperationException($"Icon '{Icon}' not found in icons.svg.");
            }

            output.Attributes.RemoveAll("icon");
            output.Attributes.Add("class", "icon");
            output.Attributes.Add("viewBox",viewBox);
            output.Content.SetHtmlContent("<use href=\"img/icons.svg#" + Icon + "\"></use>");
        }
    }
}
