using EPiServer.Core;
using EPiServer.DataAnnotations;
using System;
using EPiServer.Framework.Blobs;

namespace AlloyTemplates.Models.Media
{
    [ContentType(GUID = "EE3BD195-7CB0-4756-AB5F-E5E223CDAAAA")]
    public class MyCustomMedia : MediaData
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public virtual String Description { get; set; }

        [ImageDescriptor(Width = 48, Height = 48, Pregenerated = true)]
        public virtual Blob CustomThumbnail1 { get; set; }

        [ImageDescriptor(Width = 48, Height = 48, Pregenerated = false)]
        public virtual Blob CustomThumbnail2 { get; set; }

        public virtual Blob CustomThumbnail3 { get; set; }

        public virtual Blob CustomThumbnail4 { get; set; }
    }
}