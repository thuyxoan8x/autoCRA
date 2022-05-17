using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlloyTemplates.Automation.Models
{
    //public class VariableItem
    //{
    //    public string Key { get; set; }
    //    public object Value { get; set; }
    //    public bool Enable { get; set; } = true;
    //}
    public record VariableItem(string Key, object Value, bool Enable = true);

}
