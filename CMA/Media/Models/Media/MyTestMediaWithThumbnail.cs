using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace AlloyTemplates.Models.Media
{
    [ContentType(GUID = "0A89E464-56D4-449F-AEA8-2BF774abcdef")]
    [MediaDescriptor(ExtensionString = "bmp,wav")]
    public class MyTestMediaWithThumbnail : MediaData 
    {
        /// <summary>
        /// Gets or sets the copyright.
        /// </summary>
        /// <value>
        /// The copyright.
        /// </value>
        [MaxLength(20)]
        [MinLength(5)]
        public virtual string Copyright { get; set; }

        //
        // Summary:
        //     Gets or sets the generated thumbnail for this media.
        [ImageDescriptor(Width = 48, Height = 48, Pregenerated = true)]
        public override Blob Thumbnail { get; set; }
    }
}