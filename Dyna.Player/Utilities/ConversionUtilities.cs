using System.Diagnostics;

namespace Dyna.Player.Utilities
{
    public static class ConversionUtilities
    {
        public static string GenerateCssStyleString(object stylesObject)
        {
            Debug.WriteLine("stylesObject:");
            Debug.WriteLine(stylesObject);
            if (stylesObject is Dictionary<string, string> styles)
            {
                Debug.WriteLine("Is Dictionary<string, string>");
                var styleString = new System.Text.StringBuilder();
                Debug.WriteLine("styleString:");
                Debug.WriteLine(styleString);
                foreach (var kvp in styles)
                {
                    styleString.Append($"{kvp.Key}: {kvp.Value}; ");
                }
                return styleString.ToString();
            }
            else
            {
                Debug.WriteLine($"Error: Styles object is not a Dictionary<string, string>");
                return string.Empty; // Or handle the error as needed
            }
        }
    }
}
