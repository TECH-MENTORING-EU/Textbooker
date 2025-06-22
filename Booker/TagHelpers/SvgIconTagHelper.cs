using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Booker.Data;

namespace Booker.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("svg", Attributes = "icon")]
    public class SvgIconTagHelper : TagHelper
    {
        public required string Icon { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!Icons.ViewBoxes.TryGetValue(Icon, out var viewBox))
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
