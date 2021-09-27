using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Framework.Mvc.Models
{
    public partial class YearPickerDropDownsModel : BaseEntityModel
    {
        public YearPickerDropDownsModel()
        {
            SelectListYear = new List<SelectListItem>();
        }
        public string Attribute { get; set; }

        public string Year { get; set; }
        public IList<SelectListItem> SelectListYear { get; set; }

        public int Begin_Year { get; set; }

        public int End_Year { get; set; }

        public int SelectedYear { get; set; }
    }
}
