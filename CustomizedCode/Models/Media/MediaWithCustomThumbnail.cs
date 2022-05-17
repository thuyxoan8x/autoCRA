using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace AlloyTemplates.Models.Media
{
    [ContentType(GUID = "0A89E464-56D4-449F-AEA8-2BF774AB8333")]
    [MediaDescriptor(ExtensionString = "bmp")]
    public class MediaWithCustomThumbnail : ImageData 
    {
        /// <summary>
        /// Gets or sets the copyright.
        /// </summary>
        /// <value>
        /// The copyright.
        /// </value>
        public virtual string Copyright { get; set; }

        /// <summary>
        /// Gets or set the generated thumbnail for this media.
        /// </summary>
        [ImageDescriptor(Width = 200, Height = 200, Pregenerated = true)]
        public virtual Blob CustomThumbnail { get; set; }
    }
}