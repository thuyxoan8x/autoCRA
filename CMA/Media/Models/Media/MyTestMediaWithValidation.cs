using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace AlloyTemplates.Models.Media
{
    [ContentType(GUID = "0A89E464-56D4-449F-AEA8-2BF774ABabcd")]
    [MediaDescriptor(ExtensionString = "flv,tga")]
    public class MyTestMediaWithValidation : MediaData 
    {
        /// <summary>
        /// Gets or sets the copyright.
        /// </summary>
        /// <value>
        /// The copyright.
        /// </value>
        [Required]
        [MaxLength(20)]
        [MinLength(5)]
        public virtual string Copyright { get; set; }
    }
}