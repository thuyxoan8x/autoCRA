using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlloyTemplates.Automation.Models
{
    public record VariableItem(string Key, object Value, bool Enable = true);
}