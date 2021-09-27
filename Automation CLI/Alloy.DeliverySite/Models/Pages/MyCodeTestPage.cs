using System.ComponentModel.DataAnnotations;
using EPiServer.Content;
using System;

namespace EPiServer.Templates.Alloy.Mvc.Models.Pages
{
    /// <summary>
    /// Used for the site's start page and also acts as a container for site settings
    /// </summary>
    [ContentType(
        Id = "19671657-B684-4D95-A61F-8DD4FE60D500", BaseType = nameof(ContentTypeName.Page))]
    public class MyCodeTestPage : SitePageData
    {
        #region Property with selection UIHint
        [UIHint("selectContentReference")]
        [Display(GroupName = "SelectionTestFromModel")]
        public virtual ContentReference SelectionTestContentReference { get; set; }

        [UIHint("selectBoolean")]
        [Display(GroupName = "SelectionTestFromModel")]
        public virtual bool? SelectionTestBoolean { get; set; }

        [UIHint("selectNumber")]
        [Display(GroupName = "SelectionTestFromModel")]
        public virtual int? SelectionTestNumber { get; set; }

        [UIHint("selectFloatNumber")]
        [Display(GroupName = "SelectionTestFromModel")]
        public virtual double? SelectionTestFloatNumber { get; set; }

        [UIHint("selectString")]
        [Display(GroupName = "SelectionTestFromModel")]
        public virtual string SelectionTestString { get; set; }

        //[UIHint("selectXhtmlString")]
        //[Display(GroupName = "SelectionTestFromModel")]
        //public virtual XhtmlString SelectionTestXhtmlString { get; set; }

        //[UIHint("selectPageReference")]
        //[Display(GroupName = "SelectionTestFromModel")]
        //public virtual PageReference SelectionTestPageReference { get; set; }

        [UIHint("selectDate")]
        [Display(GroupName = "SelectionTestFromModel")]
        public virtual DateTime? SelectionTestDate { get; set; }

        //[UIHint("selectWeekDay")]
        //[Display(GroupName = "SelectionTestFromModel")]
        //public virtual Weekday SelectionTestWeekDay { get; set; }
        #endregion

        #region Require properties
        [Required]
        [UIHint("selectString")]
        [Display(GroupName = "SelectionForRequiredProp")]
        public virtual string SelectionTestStringNotNull { get; set; }
        #endregion
    }
}
