using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Booker.Data;

namespace Booker.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("svg", Attributes = "icon")]
    public class SvgIconTagHelper : TagHelper
    {
        private const string lib = "tabler";
        public required string Icon { get; set; }
        public required string Variant { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            Variant = context.AllAttributes["variant"]?.Value?.ToString() ?? "outline";

            output.Attributes.RemoveAll("icon");
            output.Attributes.RemoveAll("variant");
            output.Attributes.Add("class", "icon "+Variant);
            output.Attributes.Add("viewBox", "0 0 24 24");

            if (Variant.Equals("outline", StringComparison.OrdinalIgnoreCase))
            {
                Variant = "";
            }

            if (Variant.Equals("solid", StringComparison.OrdinalIgnoreCase))
            {
                Variant = "filled";
            }

            Variant = Variant.ToLowerInvariant();

            if (!string.IsNullOrEmpty(Variant))
            {
                Variant = "-" + Variant;
            }

            output.Content.SetHtmlContent(string.Concat("<use href=\"/img/icons", Variant, ".svg#", lib, Variant, "-", Icon, "\"></use>"));
        }
    }
}
